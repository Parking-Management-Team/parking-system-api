using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Auth.DTOs
{
    /// <summary>
    /// Đối tượng DTO (Data Transfer Object) dùng để nhận thông tin từ Client khi gửi yêu cầu đăng nhập.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Địa chỉ thư điện tử (Email) đăng nhập của tài khoản.
        /// Bắt buộc nhập và phải đúng định dạng email.
        /// </summary>
        [Required(ErrorMessage = "Email or Username is required.")]
        [MaxLength(100, ErrorMessage = "Email/Username cannot exceed 100 characters.")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Mật khẩu đăng nhập của tài khoản.
        /// Bắt buộc nhập.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must contain at least 6 characters.")]
        public string Password { get; set; } = null!;
    }
}
