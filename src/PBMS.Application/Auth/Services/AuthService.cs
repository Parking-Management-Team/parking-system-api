using PBMS.Application.Auth.DTOs;
using PBMS.Application.Auth.Interfaces;
using PBMS.Application.Contracts;
using PBMS.Application.Common.Exceptions;
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
        private readonly IRepository<Role> _roleRepository;

        // Constructor nhận vào AccountRepository và TokenService thông qua Dependency Injection
        // Constructor nhận vào các Service thông qua Dependency Injection
        public AuthService(
            IAccountRepository accountRepository,
            ITokenService tokenService,
            IGoogleAuthService googleAuthService,
            IOtpService otpService,
            IEmailService emailService,
            IRepository<Role> roleRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _googleAuthService = googleAuthService ?? throw new ArgumentNullException(nameof(googleAuthService));
            _otpService = otpService ?? throw new ArgumentNullException(nameof(otpService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
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

            // 5. Kiểm tra giới hạn gửi OTP (cooldown/lockout)
            if (_otpService.IsLockedOut(account.Email!))
            {
                throw new InvalidOperationException("This email is temporarily locked due to too many failed OTP attempts. Please try again in 15 minutes.");
            }

            if (!_otpService.CanSendOtp(account.Email!))
            {
                throw new InvalidOperationException("Please wait 60 seconds before requesting another verification code.");
            }

            // 6. Sinh mã OTP & Lưu Cache
            var otp = _otpService.GenerateAndStoreOtp(account.Email!);

            // 7. Gửi Mail qua SMTP mang thương hiệu NexPark (Emerald Theme)
            var subject = "[NexPark] - Login Verification Code";
            var body = $@"
<div style=""background-color: #f0fdf4; padding: 40px 10px; font-family: 'Inter', system-ui, -apple-system, sans-serif;"">
    <div style=""max-width: 500px; margin: 0 auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0, 0, 0, 0.06); border: 1px solid #d1fae5;"">
        
        <!-- Header / Banner - NexPark Emerald Theme -->
        <div style=""background: linear-gradient(135deg, #065f46, #047857); padding: 35px 20px; text-align: center;"">
            <h1 style=""color: #ffffff; margin: 0; font-size: 28px; font-weight: 800; letter-spacing: 1px;"">NexPark</h1>
            <p style=""color: #a7f3d0; margin: 6px 0 0 0; font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: 1.5px;"">Smart Parking Solutions</p>
        </div>
        
        <!-- Body Content -->
        <div style=""padding: 40px 32px;"">
            <h2 style=""color: #064e3b; margin-top: 0; font-size: 22px; font-weight: 700; text-align: center; letter-spacing: -0.5px;"">Verify Your Login</h2>
            <p style=""color: #475569; font-size: 15px; line-height: 1.6; text-align: center; margin-bottom: 30px;"">
                To complete your login, please use the following one-time password (OTP) verification code:
            </p>
            
            <!-- OTP Box -->
            <div style=""background-color: #ecfdf5; border: 2px dashed #6ee7b7; border-radius: 12px; padding: 22px; text-align: center; margin-bottom: 30px;"">
                <span style=""font-size: 34px; font-weight: 800; letter-spacing: 8px; color: #059669; font-family: 'Courier New', monospace; display: inline-block; padding-left: 8px;"">{otp}</span>
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
                If you did not attempt to sign in to your account, please change your password immediately.
            </p>
        </div>
        
        <!-- Footer -->
        <div style=""background-color: #f0fdf4; padding: 24px; border-top: 1px solid #d1fae5; text-align: center;"">
            <p style=""color: #059669; font-size: 12px; margin: 0 0 4px 0; font-weight: 500;"">
                Connect. Park. Go.
            </p>
            <p style=""color: #a7f3d0; font-size: 11px; margin: 0;"">
                &copy; 2026 NexPark System. All rights reserved.
            </p>
        </div>
    </div>
</div>";

            await _emailService.SendEmailAsync(account.Email!, subject, body);

            // Ném exception để báo client cần nhập OTP cho luồng login thường
            throw new LoginOtpRequiredException(account.Email!, "Login requires email verification.");
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
                // 3. TÀI KHOẢN CHƯA TỒN TẠI: Kiểm tra giới hạn gửi OTP (cooldown/lockout)
                if (_otpService.IsLockedOut(googleUser.Email))
                {
                    throw new InvalidOperationException("This email is temporarily locked due to too many failed OTP attempts. Please try again in 15 minutes.");
                }

                if (!_otpService.CanSendOtp(googleUser.Email))
                {
                    throw new InvalidOperationException("Please wait 60 seconds before requesting another verification code.");
                }

                // 4. Sinh mã OTP & Lưu Cache
                var otp = _otpService.GenerateAndStoreOtp(googleUser.Email);

                // 5. Gửi Mail qua SMTP mang thương hiệu NexPark (English Version - Emerald Theme)
                var subject = "[NexPark] - Google Registration Verification Code";
                var body = $@"
<div style=""background-color: #f0fdf4; padding: 40px 10px; font-family: 'Inter', system-ui, -apple-system, sans-serif;"">
    <div style=""max-width: 500px; margin: 0 auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0, 0, 0, 0.06); border: 1px solid #d1fae5;"">
        
        <!-- Header / Banner - NexPark Emerald Theme -->
        <div style=""background: linear-gradient(135deg, #065f46, #047857); padding: 35px 20px; text-align: center;"">
            <h1 style=""color: #ffffff; margin: 0; font-size: 28px; font-weight: 800; letter-spacing: 1px;"">NexPark</h1>
            <p style=""color: #a7f3d0; margin: 6px 0 0 0; font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: 1.5px;"">Smart Parking Solutions</p>
        </div>
        
        <!-- Body Content -->
        <div style=""padding: 40px 32px;"">
            <h2 style=""color: #064e3b; margin-top: 0; font-size: 22px; font-weight: 700; text-align: center; letter-spacing: -0.5px;"">Verify Your Google Registration</h2>
            <p style=""color: #475569; font-size: 15px; line-height: 1.6; text-align: center; margin-bottom: 30px;"">
                Thank you for choosing NexPark. Use the verification code below to complete your registration using Google:
            </p>
            
            <!-- OTP Box -->
            <div style=""background-color: #ecfdf5; border: 2px dashed #6ee7b7; border-radius: 12px; padding: 22px; text-align: center; margin-bottom: 30px;"">
                <span style=""font-size: 34px; font-weight: 800; letter-spacing: 8px; color: #059669; font-family: 'Courier New', monospace; display: inline-block; padding-left: 8px;"">{otp}</span>
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
        <div style=""background-color: #f0fdf4; padding: 24px; border-top: 1px solid #d1fae5; text-align: center;"">
            <p style=""color: #059669; font-size: 12px; margin: 0 0 4px 0; font-weight: 500;"">
                Connect. Park. Go.
            </p>
            <p style=""color: #a7f3d0; font-size: 11px; margin: 0;"">
                &copy; 2026 NexPark System. All rights reserved.
            </p>
        </div>
    </div>
</div>";

                await _emailService.SendEmailAsync(googleUser.Email, subject, body);

                // Ném exception để báo client cần nhập OTP kèm theo thông tin của Google
                throw new GoogleSignupRequiredException(googleUser.Email, googleUser.Name, "Google signup requires email verification.");
            }
            else
            {
                // 6. TÀI KHOẢN ĐÃ TỒN TẠI (Tự động liên kết): Kiểm tra xem tài khoản có đang hoạt động hay không
                if (!account.IsActive)
                {
                    throw new UnauthorizedAccessException("Your account has been locked or disabled. Please contact the administrator.");
                }
            }

            // 7. Tiến hành cấp mã Token JWT của PBMS để đăng nhập hệ thống
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

            // 5. Gửi Mail qua SMTP mang thương hiệu NexPark (English Version - Emerald Theme)
            var subject = "[NexPark] - Email Verification Code";
            var body = $@"
<div style=""background-color: #f0fdf4; padding: 40px 10px; font-family: 'Inter', system-ui, -apple-system, sans-serif;"">
    <div style=""max-width: 500px; margin: 0 auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0, 0, 0, 0.06); border: 1px solid #d1fae5;"">
        
        <!-- Header / Banner - NexPark Emerald Theme -->
        <div style=""background: linear-gradient(135deg, #065f46, #047857); padding: 35px 20px; text-align: center;"">
            <h1 style=""color: #ffffff; margin: 0; font-size: 28px; font-weight: 800; letter-spacing: 1px;"">NexPark</h1>
            <p style=""color: #a7f3d0; margin: 6px 0 0 0; font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: 1.5px;"">Smart Parking Solutions</p>
        </div>
        
        <!-- Body Content -->
        <div style=""padding: 40px 32px;"">
            <h2 style=""color: #064e3b; margin-top: 0; font-size: 22px; font-weight: 700; text-align: center; letter-spacing: -0.5px;"">Verify Your Email Address</h2>
            <p style=""color: #475569; font-size: 15px; line-height: 1.6; text-align: center; margin-bottom: 30px;"">
                Thank you for choosing NexPark. Use the verification code below to complete your registration process:
            </p>
            
            <!-- OTP Box -->
            <div style=""background-color: #ecfdf5; border: 2px dashed #6ee7b7; border-radius: 12px; padding: 22px; text-align: center; margin-bottom: 30px;"">
                <span style=""font-size: 34px; font-weight: 800; letter-spacing: 8px; color: #059669; font-family: 'Courier New', monospace; display: inline-block; padding-left: 8px;"">{otp}</span>
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
        <div style=""background-color: #f0fdf4; padding: 24px; border-top: 1px solid #d1fae5; text-align: center;"">
            <p style=""color: #059669; font-size: 12px; margin: 0 0 4px 0; font-weight: 500;"">
                Connect. Park. Go.
            </p>
            <p style=""color: #a7f3d0; font-size: 11px; margin: 0;"">
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

            // 3. Lấy Role "Driver" từ DB
            var driverRole = await _roleRepository.FirstOrDefaultAsync(r => r.RoleName == "Driver");
            if (driverRole == null)
            {
                throw new InvalidOperationException("Default role 'Driver' not found in database.");
            }

            // 4. Tạo tài khoản mới
            var account = new Account
            {
                Email = request.Email,
                Username = request.Email.Split('@')[0] + "_" + Guid.NewGuid().ToString().Substring(0, 4),
                FullName = request.FullName,
                Phone = request.Phone,
                AccountStatus = "Active",
                RoleId = driverRole.Id,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            await _accountRepository.AddAsync(account);
            await _accountRepository.SaveChangesAsync();

            // 4. Dọn dẹp Token xác thực khỏi cache
            _otpService.ClearVerificationToken(request.Email);
        }

        public async Task<LoginResponseDto> VerifyGoogleOtpAndRegisterAsync(GoogleVerifyOtpRequest request)
        {
            // 1. Xác thực tính hợp lệ của Google ID Token
            var googleUser = await _googleAuthService.VerifyTokenAsync(request.IdToken);
            if (googleUser == null)
            {
                throw new UnauthorizedAccessException("Invalid Google ID Token.");
            }

            // 2. Tìm tài khoản trong hệ thống theo email của Google trả về
            var account = await _accountRepository.GetByEmailAsync(googleUser.Email);

            if (account == null)
            {
                // 3. Xác thực OTP
                var result = _otpService.VerifyOtp(googleUser.Email, request.Otp);
                if (!result.IsSuccess)
                {
                    throw new InvalidOperationException(result.Message);
                }

                // 4. Lấy Role "Driver" từ DB
                var driverRole = await _roleRepository.FirstOrDefaultAsync(r => r.RoleName == "Driver");
                if (driverRole == null)
                {
                    throw new InvalidOperationException("Default role 'Driver' not found in database.");
                }

                // 5. Tạo tài khoản mới
                account = new Account
                {
                    Email = googleUser.Email,
                    Username = googleUser.Email.Split('@')[0] + "_" + Guid.NewGuid().ToString().Substring(0, 4),
                    FullName = googleUser.Name,
                    AccountStatus = "Active",
                    RoleId = driverRole.Id,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString())
                };

                await _accountRepository.AddAsync(account);
                await _accountRepository.SaveChangesAsync();
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

        /// <summary>
        /// Xác thực mã OTP và hoàn tất đăng nhập cho email thường.
        /// </summary>
        public async Task<LoginResponseDto> VerifyLoginOtpAsync(LoginVerifyOtpRequest request)
        {
            // 1. Tìm tài khoản theo Email
            var account = await _accountRepository.GetByEmailAsync(request.Email);
            if (account == null)
            {
                throw new UnauthorizedAccessException("Incorrect email or password.");
            }

            // 2. Kiểm tra trạng thái hoạt động của tài khoản
            if (!account.IsActive)
            {
                throw new UnauthorizedAccessException("Your account has been locked or disabled. Please contact the administrator.");
            }

            // 3. Kiểm tra mật khẩu (để bảo vệ chống brute force OTP trực tiếp)
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Incorrect email or password.");
            }

            // 4. Xác thực OTP
            var verifyResult = _otpService.VerifyOtp(request.Email, request.Otp);
            if (!verifyResult.IsSuccess)
            {
                throw new InvalidOperationException(verifyResult.Message);
            }

            // 5. Sinh JWT Token
            var (token, expiration) = _tokenService.GenerateToken(account);

            // 6. Trả về LoginResponseDto
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
    }
}
