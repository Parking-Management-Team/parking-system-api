
using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Accounts
{
    /// <summary>
    /// DTO nhận dữ liệu yêu cầu đổi mật khẩu 
    /// <summary>
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Old password is required.")]
        public string OldPassword { get; set; } = null!;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters long.")]
        public string NewPassword { get; set; } = null!;



    }
}