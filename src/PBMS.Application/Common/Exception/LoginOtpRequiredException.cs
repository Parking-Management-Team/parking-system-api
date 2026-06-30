using System;

namespace PBMS.Application.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when a normal login attempt requires OTP email verification to complete.
    /// </summary>
    public class LoginOtpRequiredException : Exception
    {
        public string Email { get; }

        public LoginOtpRequiredException(string email, string message) : base(message)
        {
            Email = email;
        }
    }
}
