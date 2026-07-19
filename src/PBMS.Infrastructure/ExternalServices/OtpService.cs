using Microsoft.Extensions.Caching.Memory;
using PBMS.Application.Auth.Interfaces;
using System;
using System.Security.Cryptography;

namespace PBMS.Infrastructure.ExternalServices
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private const int MaxFailedAttempts = 5;

        public OtpService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool CanSendOtp(string email)
        {
            // Nếu tìm thấy key cooldown, tức là user vừa gửi OTP trong vòng 60s trước -> báo chặn
            return !_cache.TryGetValue($"otp_cooldown:{email}", out _);
        }

        public string GenerateAndStoreOtp(string email)
        {
            // 1. Sinh OTP ngẫu nhiên 6 số bảo mật
            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            // 2. Lưu OTP vào Memory Cache với hạn 5 phút
            _cache.Set($"otp:{email}", otp, TimeSpan.FromMinutes(5));

            // 3. Đặt thời gian cooldown 60 giây chặn gửi spam
            _cache.Set($"otp_cooldown:{email}", "active", TimeSpan.FromSeconds(60));

            // 4. Reset số lần nhập sai về 0
            _cache.Set($"otp_attempts:{email}", 0, TimeSpan.FromMinutes(5));

            return otp;
        }

        public bool IsLockedOut(string email)
        {
            // Kiểm tra xem email có bị khóa tạm thời 15 phút không
            return _cache.TryGetValue($"otp_lockout:{email}", out _);
        }

        public (bool IsSuccess, string? Message, string? VerificationToken) VerifyOtp(string email, string otp)
        {
            // 1. Kiểm tra trạng thái khóa
            if (IsLockedOut(email))
            {
                return (false, "This email is temporarily locked due to too many failed OTP attempts. Please try again in 15 minutes.", null);
            }

            // 2. Lấy OTP lưu trong bộ nhớ
            if (!_cache.TryGetValue($"otp:{email}", out string? storedOtp) || string.IsNullOrEmpty(storedOtp))
            {
                return (false, "OTP has expired or is invalid. Please request a new code.", null);
            }

            // 3. So khớp mã OTP
            if (storedOtp != otp)
            {
                // Tăng số lần thử sai
                _cache.TryGetValue($"otp_attempts:{email}", out int attempts);
                attempts++;

                if (attempts >= MaxFailedAttempts)
                {
                    // Đạt 5 lần sai -> Khóa 15 phút
                    _cache.Set($"otp_lockout:{email}", "locked", TimeSpan.FromMinutes(15));
                    _cache.Remove($"otp:{email}");
                    return (false, "Too many failed attempts. Your email verification has been locked for 15 minutes.", null);
                }

                _cache.Set($"otp_attempts:{email}", attempts, TimeSpan.FromMinutes(5));
                return (false, $"Invalid OTP. You have {MaxFailedAttempts - attempts} attempts remaining.", null);
            }

            // 4. OTP đúng -> Sinh Verification Token (GUID) hợp lệ trong 10 phút
            var verificationToken = Guid.NewGuid().ToString("N");
            _cache.Set($"otp_verified:{email}", verificationToken, TimeSpan.FromMinutes(10));

            // Xóa mã OTP và số lần thử sai cũ
            _cache.Remove($"otp:{email}");
            _cache.Remove($"otp_attempts:{email}");

            return (true, null, verificationToken);
        }

        public bool ValidateVerificationToken(string email, string token)
        {
            if (_cache.TryGetValue($"otp_verified:{email}", out string? storedToken))
            {
                return !string.IsNullOrEmpty(storedToken) && storedToken == token;
            }
            return false;
        }

        public void ClearVerificationToken(string email)
        {
            _cache.Remove($"otp_verified:{email}");
        }
    }
}
