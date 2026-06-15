namespace PBMS.Application.Accounts.DTOs
{
    /// <summary>
    /// DTO chứa các thông tin được phép cập nhật của một Account.
    /// </summary>
    public class UpdateAccountDto
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public int? RoleId { get; set; } // Nullable: Nếu không gửi lên thì không đổi Role
        public string? AccountStatus { get; set; } // Nullable: Nếu không gửi lên thì không đổi Trạng thái
    }
}
