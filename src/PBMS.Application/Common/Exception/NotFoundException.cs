namespace PBMS.Application.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested entity is not found in the database.
    /// This exception is mapped to HTTP 404 (Not Found) response by the ExceptionHandlingMiddleware.
    /// </summary>
    public class NotFoundException : AppException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        /// <param name="entityName">The name of the entity type that was not found.</param>
        /// <param name="key">The identifier or key of the entity that was not found.</param>
        public NotFoundException(string entityName, object key)
            : base("NOT_FOUND", $"Entity '{entityName}' with key '{key}' was not found.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class with a custom message.
        /// </summary>
        /// <param name="message">The custom error message.</param>
        public NotFoundException(string message)
            : base("NOT_FOUND", message)
        {
        }
    }
}
