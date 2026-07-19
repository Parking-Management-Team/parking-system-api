using System;

namespace PBMS.Application.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when a Google login attempt requires OTP email verification before registration.
    /// </summary>
    public class GoogleSignupRequiredException : Exception
    {
        public string Email { get; }
        public string FullName { get; }

        public GoogleSignupRequiredException(string email, string fullName, string message) : base(message)
        {
            Email = email;
            FullName = fullName;
        }
    }
}
