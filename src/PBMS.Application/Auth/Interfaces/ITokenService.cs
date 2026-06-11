using PBMS.Domain.Entities;
using System;

namespace PBMS.Application.Auth.Interfaces
{
    /// <summary>
    /// Giao diện dịch vụ xử lý JWT Token.
    /// Định nghĩa phương thức phát hành mã xác thực cho tài khoản.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Tạo mới mã JWT Access Token kèm theo thời gian hết hạn dựa trên thông tin tài khoản.
        /// </summary>
        /// <param name="account">Thực thể Account chứa thông tin định danh và vai trò (Role).</param>
        /// <returns>Một Tuple chứa: Token dạng chuỗi mã hóa và Expiration là thời điểm hết hạn.</returns>
        (string Token, DateTime Expiration) GenerateToken(Account account);
    }
}
