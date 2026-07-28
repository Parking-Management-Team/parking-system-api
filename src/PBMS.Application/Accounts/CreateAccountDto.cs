using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Accounts
{
    /// <summary>
    /// DTO chứa thông tin truyền lên khi tạo mới một tài khoản người dùng từ Admin/Manager.
    /// </summary>
    public class CreateAccountDto
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; } = null!;

        public string? Password { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = null!;

        public string? Phone { get; set; }

        [Required(ErrorMessage = "RoleId is required.")]
        public int RoleId { get; set; }
    }
}
