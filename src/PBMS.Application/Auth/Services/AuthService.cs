using PBMS.Application.Auth.DTOs;
using PBMS.Application.Auth.Interfaces;
using PBMS.Application.Contracts;
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

        // Constructor nhận vào AccountRepository và TokenService thông qua Dependency Injection
        public AuthService(IAccountRepository accountRepository, ITokenService tokenService)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
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
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không chính xác.");
            }

            // 3. Kiểm tra trạng thái hoạt động của tài khoản
            // Nếu tài khoản đã bị Block hoặc không hoạt động, từ chối đăng nhập
            if (!account.IsActive)
            {
                throw new UnauthorizedAccessException("Tài khoản của bạn đã bị khóa hoặc vô hiệu hóa. Vui lòng liên hệ Quản trị viên.");
            }

            // 4. Kiểm tra sự trùng khớp của mật khẩu gửi lên và mật khẩu mã hóa lưu trong DB
            // Sử dụng thư viện BCrypt để mã hóa một chiều an toàn chống tấn công Brute force
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không chính xác.");
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
    }
}
