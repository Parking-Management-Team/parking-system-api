namespace PBMS.Application.Accounts.DTOs
{
    /// <summary>
    /// DTO chứa các thông tin được phép cập nhật của một Account.
    /// </summary>
    public class UpdateAccountDto
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        
        // Nullable: Nếu không muốn đổi vai trò thì gửi null
        public int? RoleId { get; set; }
        
        // Nullable: Nếu không muốn đổi trạng thái thì gửi null
        public string? AccountStatus { get; set; }
    }
}
