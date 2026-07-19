namespace PBMS.Application.Auth.Interfaces
{
    public interface IOtpService
    {
        bool CanSendOtp(string email);
        string GenerateAndStoreOtp(string email);
        bool IsLockedOut(string email);
        (bool IsSuccess, string? Message, string? VerificationToken) VerifyOtp(string email, string otp);
        bool ValidateVerificationToken(string email, string token);
        void ClearVerificationToken(string email);
    }
}
