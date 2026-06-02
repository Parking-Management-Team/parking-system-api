using PBMS.Application.Auth.DTOs;
using System.Threading.Tasks;

namespace PBMS.Application.Auth.Interfaces
{
    /// <summary>
    /// Giao diện dịch vụ nghiệp vụ liên quan tới Xác thực tài khoản (Authentication).
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Xử lý yêu cầu đăng nhập của tài khoản.
        /// Thực hiện xác thực thông tin email, mật khẩu và trạng thái hoạt động của tài khoản.
        /// </summary>
        /// <param name="request">Yêu cầu đăng nhập chứa Email và Mật khẩu.</param>
        /// <returns>Đối tượng LoginResponseDto chứa mã JWT Token và thông tin định danh người dùng.</returns>
        Task<LoginResponseDto> LoginAsync(LoginRequest request);
    }
}
