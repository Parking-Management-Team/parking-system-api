using System;

namespace PBMS.Application.Accounts.DTOs
{
    /// <summary>
    /// DTO đại diện cho dữ liệu Account trả về cho Client.
    /// Giúp ẩn đi các thông tin nhạy cảm như PasswordHash.
    /// </summary>
    public class AccountDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int RoleId { get; set; }

        // Tên vai trò (Admin, Manager, Driver...) lấy từ thực thể Role liên kết
        public string RoleName { get; set; } = null!;
        public string AccountStatus { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
