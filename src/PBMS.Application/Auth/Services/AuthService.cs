using PBMS.Application.Auth.DTOs;
using PBMS.Application.Auth.Interfaces;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace PBMS.Application.Auth.Services
{
    /// <summary>
    /// Triển khai dịch vụ xác thực tài khoản IAuthService.
    /// Chứa toàn bộ nghiệp vụ kiểm tra thông tin đăng nhập, đối chiếu mật khẩu và cấp Token.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITokenService _tokenService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;

        // Constructor nhận vào AccountRepository và TokenService thông qua Dependency Injection
        // Constructor nhận vào các Service thông qua Dependency Injection
        public AuthService(
            IAccountRepository accountRepository,
            ITokenService tokenService,
            IGoogleAuthService googleAuthService,
            IOtpService otpService,      // <-- Thêm tham số này
            IEmailService emailService)  // <-- Thêm tham số này
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _googleAuthService = googleAuthService ?? throw new ArgumentNullException(nameof(googleAuthService));
            _otpService = otpService ?? throw new ArgumentNullException(nameof(otpService));       // <-- Thêm dòng này
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService)); // <-- Thêm dòng này
        }


        /// <summary>
        /// Xử lý logic đăng nhập:
        /// 1. Tìm tài khoản theo Email.
        /// 2. Kiểm tra trạng thái hoạt động (IsActive).
        /// 3. So khớp mật khẩu đã mã hóa bằng BCrypt.
        /// 4. Phát hành JWT Token nếu hợp lệ.
        /// </summary>
        public async Task<LoginResponseDto> LoginAsync(LoginRequest request)
        {
            // 1. Tìm kiếm tài khoản từ Database dựa theo Email nhận được từ Client
            var account = await _accountRepository.GetByEmailAsync(request.Email);

            // 2. Nếu không tìm thấy tài khoản, ném ngoại lệ UnauthorizedAccessException
            // (Thông báo chung chung để tránh rò rỉ thông tin tài khoản tồn tại trong hệ thống)
            if (account == null)
            {
                throw new UnauthorizedAccessException("Incorrect email or password.");
            }

            // 3. Kiểm tra trạng thái hoạt động của tài khoản
            // Nếu tài khoản đã bị Block hoặc không hoạt động, từ chối đăng nhập
            if (!account.IsActive)
            {
                throw new UnauthorizedAccessException("Your account has been locked or disabled. Please contact the administrator.");
            }

            // 4. Kiểm tra sự trùng khớp của mật khẩu gửi lên và mật khẩu mã hóa lưu trong DB
            // Sử dụng thư viện BCrypt để mã hóa một chiều an toàn chống tấn công Brute force
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Incorrect email or password.");
            }

            // 5. Nếu tất cả thông tin đều chính xác, tiến hành sinh Token JWT thông qua TokenService
            var (token, expiration) = _tokenService.GenerateToken(account);

            // 6. Trả về DTO chứa mã Token và thông tin định danh của người dùng
            return new LoginResponseDto
            {
                Token = token,
                Expiration = expiration,
                AccountId = account.Id,
                Username = account.Username,
                Email = account.Email,
                FullName = account.FullName,
                RoleName = account.Role.RoleName // Tên vai trò được lấy nhờ kỹ thuật Eager Loading (.Include) trước đó
            };
        }

        /// <summary>
        /// Xử lý logic đăng nhập bằng Google OAuth2.
        /// Xác thực ID Token của Google, tự động tạo tài khoản hoặc liên kết nếu hợp lệ.
        /// </summary>
        public async Task<LoginResponseDto> LoginWithGoogleAsync(GoogleLoginRequest request)
        {
            // 1. Xác thực tính hợp lệ của Google ID Token nhận từ Client
            var googleUser = await _googleAuthService.VerifyTokenAsync(request.IdToken);
            if (googleUser == null)
            {
                throw new UnauthorizedAccessException("Invalid Google ID Token.");
            }

            // 2. Tìm tài khoản trong hệ thống theo email của Google trả về
            var account = await _accountRepository.GetByEmailAsync(googleUser.Email);

            if (account == null)
            {
                // 3. TÀI KHOẢN CHƯA TỒN TẠI: Tự động đăng ký mới với vai trò là Driver (RoleId = 3)
                account = new Account
                {
                    Email = googleUser.Email,
                    // Tên đăng nhập độc nhất tự sinh từ email
                    Username = googleUser.Email.Split('@')[0] + "_" + Guid.NewGuid().ToString().Substring(0, 4),
                    FullName = googleUser.Name,
                    AccountStatus = "Active",
                    RoleId = 3, // Vai trò Driver mặc định
                    // Hash một GUID ngẫu nhiên làm mật khẩu để không thể dùng cách đăng nhập mật khẩu truyền thống
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString())
                };

                await _accountRepository.AddAsync(account);
                await _accountRepository.SaveChangesAsync();
            }
            else
            {
                // 4. TÀI KHOẢN ĐÃ TỒN TẠI (Tự động liên kết): Kiểm tra xem tài khoản có đang hoạt động hay không
                if (!account.IsActive)
                {
                    throw new UnauthorizedAccessException("Your account has been locked or disabled. Please contact the administrator.");
                }
            }

            // 5. Tiến hành cấp mã Token JWT của PBMS để đăng nhập hệ thống
            var (token, expiration) = _tokenService.GenerateToken(account);

            return new LoginResponseDto
            {
                Token = token,
                Expiration = expiration,
                AccountId = account.Id,
                Username = account.Username,
                Email = account.Email,
                FullName = account.FullName,
                RoleName = account.Role?.RoleName ?? "Driver"
            };
        }

        // Cập nhật Constructor để tiêm thêm 2 service này...
        public async Task SendOtpForRegisterAsync(string email)
        {
            // 1. Kiểm tra tài khoản đã tồn tại chưa
            var existingAccount = await _accountRepository.GetByEmailAsync(email);
            if (existingAccount != null)
            {
                throw new InvalidOperationException("Email is already registered in the system.");
            }

            // 2. Kiểm tra cooldown (60s)
            if (!_otpService.CanSendOtp(email))
            {
                throw new InvalidOperationException("Please wait 60 seconds before requesting another verification code.");
            }

            // 3. Kiểm tra trạng thái khóa (Lockout)
            if (_otpService.IsLockedOut(email))
            {
                throw new InvalidOperationException("This email is locked due to too many failed OTP attempts. Please try again in 15 minutes.");
            }

            // 4. Sinh OTP & Lưu Cache
            var otp = _otpService.GenerateAndStoreOtp(email);

            // 5. Gửi Mail qua SMTP mang thương hiệu NexPark (English Version)
            var subject = "[NexPark] - Email Verification Code";
            var body = $@"
<div style=""background-color: #f8fafc; padding: 40px 10px; font-family: 'Inter', system-ui, -apple-system, sans-serif;"">
    <div style=""max-width: 500px; margin: 0 auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0, 0, 0, 0.03); border: 1px solid #e2e8f0;"">
        
        <!-- Header / Banner - NexPark Theme -->
        <div style=""background: linear-gradient(135deg, #1e293b, #0f172a); padding: 35px 20px; text-align: center;"">
            <div style=""display: inline-flex; align-items: center; justify-content: center; background-color: rgba(56, 189, 248, 0.1); border-radius: 10px; width: 44px; height: 44px; margin: 0 auto 12px auto; font-weight: 800; font-size: 22px; color: #38bdf8;"">P</div>
            <h1 style=""color: #ffffff; margin: 0; font-size: 26px; font-weight: 800; letter-spacing: 0.5px;"">NexPark</h1>
            <p style=""color: #94a3b8; margin: 4px 0 0 0; font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: 1.5px;"">Smart Parking Solutions</p>
        </div>
        
        <!-- Body Content -->
        <div style=""padding: 40px 32px;"">
            <h2 style=""color: #0f172a; margin-top: 0; font-size: 22px; font-weight: 700; text-align: center; letter-spacing: -0.5px;"">Verify Your Email Address</h2>
            <p style=""color: #475569; font-size: 15px; line-height: 1.6; text-align: center; margin-bottom: 30px;"">
                Thank you for choosing NexPark. Use the verification code below to complete your registration process:
            </p>
            
            <!-- OTP Box -->
            <div style=""background-color: #f1f5f9; border: 1px dashed #cbd5e1; border-radius: 12px; padding: 22px; text-align: center; margin-bottom: 30px;"">
                <span style=""font-size: 34px; font-weight: 800; letter-spacing: 8px; color: #2563eb; font-family: 'Courier New', monospace; display: inline-block; padding-left: 8px;"">{otp}</span>
            </div>
            
            <!-- Security Notice -->
            <div style=""border-left: 4px solid #f59e0b; background-color: #fef3c7; padding: 16px; border-radius: 6px; margin-bottom: 30px;"">
                <p style=""color: #b45309; font-size: 13px; font-weight: 700; margin: 0 0 4px 0; line-height: 1.4;"">
                    ⚠️ Security Notice:
                </p>
                <p style=""color: #6b7280; font-size: 13px; margin: 0; line-height: 1.5;"">
                    This one-time password (OTP) is valid for <strong>5 minutes</strong>. Never share this code with anyone, including NexPark staff.
                </p>
            </div>
            
            <p style=""color: #94a3b8; font-size: 12px; text-align: center; line-height: 1.5; margin: 0;"">
                If you did not request this verification code, please ignore this email safely.
            </p>
        </div>
        
        <!-- Footer -->
        <div style=""background-color: #f8fafc; padding: 24px; border-top: 1px solid #e2e8f0; text-align: center;"">
            <p style=""color: #94a3b8; font-size: 12px; margin: 0 0 4px 0;"">
                Connect. Park. Go.
            </p>
            <p style=""color: #cbd5e1; font-size: 11px; margin: 0;"">
                &copy; 2026 NexPark System. All rights reserved.
            </p>
        </div>
    </div>
</div>";

            await _emailService.SendEmailAsync(email, subject, body);
        }

        public async Task<string> VerifyOtpForRegisterAsync(string email, string otp)
        {
            var (isSuccess, message, verificationToken) = _otpService.VerifyOtp(email, otp);
            if (!isSuccess)
            {
                throw new InvalidOperationException(message ?? "OTP verification failed.");
            }

            return verificationToken!;
        }

        public async Task RegisterVerifiedUserAsync(RegisterVerifiedRequest request)
        {
            // 1. Kiểm tra Token đã xác thực thành công ở bước trước chưa
            var isValid = _otpService.ValidateVerificationToken(request.Email, request.VerificationToken);
            if (!isValid)
            {
                throw new InvalidOperationException("Email verification token is invalid or has expired.");
            }

            // 2. Kiểm tra lại email
            var existingAccount = await _accountRepository.GetByEmailAsync(request.Email);
            if (existingAccount != null)
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            // 3. Tạo tài khoản mới (RoleId = 3 cho Driver mặc định)
            var account = new Account
            {
                Email = request.Email,
                Username = request.Email.Split('@')[0] + "_" + Guid.NewGuid().ToString().Substring(0, 4),
                FullName = request.FullName,
                Phone = request.Phone,
                AccountStatus = "Active",
                RoleId = 3,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            await _accountRepository.AddAsync(account);
            await _accountRepository.SaveChangesAsync();

            // 4. Dọn dẹp Token xác thực khỏi cache
            _otpService.ClearVerificationToken(request.Email);
        }

    }
}
