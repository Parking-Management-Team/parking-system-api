using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using PBMS.Application.Auth.Interfaces;
using System;
using System.Threading.Tasks;

namespace PBMS.Infrastructure.ExternalServices
{
    /// <summary>
    /// Triển khai dịch vụ xác thực Google OAuth2.
    /// </summary>
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;

        public GoogleAuthService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Xác thực ID Token của Google và lấy thông tin người dùng.
        /// </summary>
        public async Task<GoogleUserInfo?> VerifyTokenAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                return null;
            }

            try
            {
                // Lấy Google Client ID từ cấu hình appsetting.json
                var clientId = _configuration["Google:ClientId"] ?? "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com";

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                };

                // Xác thực token
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);


                // Trả về thông tin người dùng giải mã được 
                return new GoogleUserInfo
                {
                    Email = payload.Email,
                    Name = payload.Name,
                    PictureUrl = payload.Picture // Gán link ảnh đại diện từ Google vào PictureURL
                };
            }
            catch (Exception)
            {
                // Trả về null nếu token không hợp lệ ( hết hạn , sai chũ kĩ , sai Audience....)
                return null;
            }
        }
    }
}
