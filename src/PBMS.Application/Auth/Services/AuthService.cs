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

        // Constructor nhận vào AccountRepository và TokenService thông qua Dependency Injection
        public AuthService(IAccountRepository accountRepository, ITokenService tokenService, IGoogleAuthService googleAuthService)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _googleAuthService = googleAuthService ?? throw new ArgumentNullException(nameof(googleAuthService));
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
                    AccountStatus = AccountStatus.Active,
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
    }
}
