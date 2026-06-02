using System.Collections.Generic;
using System.Linq;

namespace PBMS.Application.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when request data validation fails.
    /// This exception is mapped to HTTP 400 (Bad Request) response by the ExceptionHandlingMiddleware.
    /// </summary>
    public class ValidationException : AppException
    {
        /// <summary>
        /// Gets the collection of validation errors.
        /// </summary>
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        public ValidationException()
            : base("VALIDATION_ERROR", "One or more validation errors occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with validation errors.
        /// </summary>
        /// <param name="errors">A dictionary mapping property names to their validation error messages.</param>
        public ValidationException(IDictionary<string, string[]> errors)
            : base("VALIDATION_ERROR", "One or more validation errors occurred.")
        {
            Errors = errors.AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a custom message.
        /// </summary>
        /// <param name="message">The custom error message.</param>
        public ValidationException(string message)
            : base("VALIDATION_ERROR", message)
        {
            Errors = new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with custom message and errors.
        /// </summary>
        /// <param name="message">The custom error message.</param>
        /// <param name="errors">A dictionary mapping property names to their validation error messages.</param>
        public ValidationException(string message, IDictionary<string, string[]> errors)
            : base("VALIDATION_ERROR", message)
        {
            Errors = errors.AsReadOnly();
        }
    }
}
