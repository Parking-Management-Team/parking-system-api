namespace PBMS.Application.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when a user lacks the required permissions to perform an operation.
    /// This exception is mapped to HTTP 403 (Forbidden) response by the ExceptionHandlingMiddleware.
    /// </summary>
    public class ForbiddenException : AppException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
        /// </summary>
        /// <param name="message">The error message explaining why the action is forbidden.</param>
        public ForbiddenException(string message)
            : base("FORBIDDEN", message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenException"/> class with resource and reason.
        /// </summary>
        /// <param name="resourceName">The name of the resource being accessed.</param>
        /// <param name="action">The action that was attempted.</param>
        public ForbiddenException(string resourceName, string action)
            : base("FORBIDDEN", $"You do not have permission to {action} '{resourceName}'.")
        {
        }
    }
}
