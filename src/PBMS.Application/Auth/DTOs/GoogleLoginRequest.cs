using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Auth.DTOs
{
    /// <summary>
    /// Đối tượng DTO nhận thông tin đăng nhập Google OAuth2 từ Client.
    /// </summary>

    public class GoogleLoginRequest
    {
        /// <summary>
        /// Mã ID Token (dạng JWT) do Google cấp cho Frontend sau khi đăng nhập tàhnh công
        /// </summary>
        [Required(ErrorMessage = "Google ID Token is required.")]
        public string IdToken { get; set; } = null!;
    }

}
