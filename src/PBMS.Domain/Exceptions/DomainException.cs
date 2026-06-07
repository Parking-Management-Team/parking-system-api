using System;

namespace PBMS.Domain.Exceptions
{
    /// <summary>
    /// Base class for all domain/business exceptions.
    /// Represents errors that occur due to business logic violations or domain constraints.
    /// </summary>
    public class DomainException : Exception
    {
        /// <summary>
        /// Gets the error code for the exception.
        /// </summary>
        public string ErrorCode { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public DomainException(string message) : base(message)
        {
            ErrorCode = "DOMAIN_ERROR";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class with error code.
        /// </summary>
        /// <param name="errorCode">The specific error code to identify the type of domain error.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public DomainException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class with an inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public DomainException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "DOMAIN_ERROR";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class with error code and inner exception.
        /// </summary>
        /// <param name="errorCode">The specific error code to identify the type of domain error.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public DomainException(string errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
