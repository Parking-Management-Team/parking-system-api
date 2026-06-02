using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PBMS.Application.Auth.Interfaces;
using PBMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PBMS.Infrastructure.ExternalServices
{
    /// <summary>
    /// Triển khai dịch vụ ITokenService để phát hành mã JWT Access Token.
    /// Đọc cấu hình từ appsettings.json và tạo chữ ký số bảo mật cho Token.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        // Constructor nhận vào IConfiguration để đọc các tham số JWT từ file appsettings.json
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Tạo mới mã JWT Access Token dựa trên thông tin Account và Role liên kết.
        /// </summary>
        /// <param name="account">Thực thể Account cần cấp quyền.</param>
        /// <returns>Một Tuple chứa: Chuỗi JWT Token mã hóa và Thời điểm Token hết hạn.</returns>
        public (string Token, DateTime Expiration) GenerateToken(Account account)
        {
            // 1. Lấy thông tin cấu hình bảo mật từ file cấu hình appsettings.json
            // Khóa bí mật (Key) dùng để ký mã hóa Token (Yêu cầu độ dài tối thiểu 32 ký tự để đảm bảo thuật toán HmacSha256)
            var secretKey = _configuration["Jwt:Key"] ?? "A_Super_Secret_Key_For_JWT_Auth_System_PBMS_Project_2026_SWP391";
            var issuer = _configuration["Jwt:Issuer"] ?? "PBMS";
            var audience = _configuration["Jwt:Audience"] ?? "PBMSUsers";
            
            // Thời gian hết hạn của token (mặc định là 1440 phút ~ 24 giờ)
            var expiryInMinutes = double.TryParse(_configuration["Jwt:ExpiryInMinutes"], out var parsedExpiry)
                ? parsedExpiry
                : 1440;

            // 2. Chuyển đổi Khóa bí mật từ dạng Chuỗi sang mảng byte bảo mật
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            
            // Sử dụng thuật toán HMAC-SHA256 để làm chữ ký điện tử cho Token, ngăn chặn việc giả mạo gói tin JWT từ Client
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. Định nghĩa danh sách các Claims (Khẳng định thông tin) được lưu trữ trực tiếp bên trong Token
            // Giúp API Controller giải mã và lấy nhanh thông tin User mà không cần truy vấn lại DB liên tục
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()), // ID của tài khoản
                new Claim(JwtRegisteredClaimNames.UniqueName, account.Username), // Tên tài khoản độc nhất
                new Claim(ClaimTypes.Role, account.Role?.RoleName ?? "Customer") // Vai trò hệ thống để phân quyền [Authorize(Roles = "...")]
            };

            // Nếu tài khoản có địa chỉ Email, nhúng thông tin Email vào claim
            if (!string.IsNullOrWhiteSpace(account.Email))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, account.Email));
            }

            // Nếu tài khoản có Họ và tên, nhúng thông tin Họ và tên vào claim
            if (!string.IsNullOrWhiteSpace(account.FullName))
            {
                claims.Add(new Claim(ClaimTypes.Name, account.FullName));
            }

            // 4. Tính toán thời điểm hết hạn của Token
            var expiration = DateTime.UtcNow.AddMinutes(expiryInMinutes);

            // 5. Khởi tạo đối tượng JWT Security Token mô tả đầy đủ cấu trúc Token
            var tokenOptions = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            // 6. Mã hóa đối tượng JWT thành dạng chuỗi kí tự Base64 (Chuỗi Token hoàn chỉnh)
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return (tokenString, expiration);
        }
    }
}
