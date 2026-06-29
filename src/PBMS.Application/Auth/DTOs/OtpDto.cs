using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.Auth.DTOs
{
    public class SendOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }

    public class VerifyOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = null!;
    }

    public class RegisterVerifiedRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        public string? Phone { get; set; }

        [Required]
        public string VerificationToken { get; set; } = null!;
    }

    public class GoogleVerifyOtpRequest
    {
        [Required]
        public string IdToken { get; set; } = null!;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = null!;
    }
}
