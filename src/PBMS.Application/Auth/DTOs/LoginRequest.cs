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
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không đúng định dạng.")]
        [MaxLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Mật khẩu đăng nhập của tài khoản.
        /// Bắt buộc nhập.
        /// </summary>
        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải chứa ít nhất 6 ký tự.")]
        public string Password { get; set; } = null!;
    }
}
