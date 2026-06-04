using System;

namespace PBMS.Application.Auth.DTOs
{
    /// <summary>
    /// Đối tượng DTO trả về cho Client sau khi đăng nhập thành công.
    /// Chứa mã JWT Token và các thông tin cơ bản của tài khoản phục vụ hiển thị ở Frontend.
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// Chuỗi mã JWT Access Token dùng để đính kèm vào Header của các API yêu cầu xác thực.
        /// </summary>
        public string Token { get; set; } = null!;

        /// <summary>
        /// Thời điểm mã JWT Token hết hạn.
        /// </summary>
        public DateTime Expiration { get; set; }

        /// <summary>
        /// ID của tài khoản đăng nhập.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Tên người dùng (Username) của tài khoản.
        /// </summary>
        public string Username { get; set; } = null!;

        /// <summary>
        /// Địa chỉ email của tài khoản.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Họ và tên đầy đủ của chủ tài khoản.
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Tên vai trò (Role) của người dùng trong hệ thống (ví dụ: Admin, Staff, Customer, ...).
        /// Dùng để phân quyền hiển thị giao diện ở Frontend.
        /// </summary>
        public string RoleName { get; set; } = null!;
    }
}
