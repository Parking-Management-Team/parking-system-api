using PBMS.Domain.Exceptions;

namespace PBMS.Application.Common.Exceptions
{
    /// <summary>
    /// Base class for all application layer exceptions.
    /// This exception is used for application-specific errors that do not derive from domain exceptions.
    /// </summary>
    public class AppException : DomainException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public AppException(string message) : base(message)
        {
            ErrorCode = "APP_ERROR";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppException"/> class with error code.
        /// </summary>
        /// <param name="errorCode">The specific error code to identify the type of application error.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public AppException(string errorCode, string message) : base(errorCode, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppException"/> class with an inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public AppException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "APP_ERROR";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppException"/> class with error code and inner exception.
        /// </summary>
        /// <param name="errorCode">The specific error code to identify the type of application error.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public AppException(string errorCode, string message, Exception innerException) : base(errorCode, message, innerException)
        {
        }
    }
}
