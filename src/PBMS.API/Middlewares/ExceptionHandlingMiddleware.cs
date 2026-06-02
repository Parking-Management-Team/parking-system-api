using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PBMS.Application.Common;
using PBMS.Application.Common.Exceptions;

namespace PBMS.API.Middlewares
{
    /// <summary>
    /// Middleware that handles exceptions thrown during request processing.
    /// This middleware serves as the final checkpoint for catching and formatting all exceptions
    /// into standardized error responses.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        /// <summary>
        /// Invokes the middleware to handle the HTTP request and catch any exceptions.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        /// <summary>
        /// Handles the exception by mapping it to an appropriate HTTP response.
        /// Different exception types are mapped to their corresponding HTTP status codes and error messages.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <param name="exception">The exception that was thrown.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Initialize response properties
            BaseResponse<object> response;
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

            // Map exception to appropriate response and status code
            if (exception is NotFoundException notFoundEx)
            {
                statusCode = HttpStatusCode.NotFound;
                response = BaseResponse<object>.Fail(
                    notFoundEx.ErrorCode,
                    exception.Message
                );
            }
            else if (exception is ValidationException validationEx)
            {
                statusCode = HttpStatusCode.BadRequest;
                response = BaseResponse<object>.ValidationFail(
                    validationEx.ErrorCode,
                    exception.Message,
                    validationEx.Errors
                );
            }
            else if (exception is ForbiddenException forbiddenEx)
            {
                statusCode = HttpStatusCode.Forbidden;
                response = BaseResponse<object>.Fail(
                    forbiddenEx.ErrorCode,
                    exception.Message
                );
            }
            // Xử lý xung đột đồng thời (Concurrency Conflict):
            // Khi 2 người cùng sửa/xóa cùng 1 bản ghi, EF Core ném DbUpdateConcurrencyException.
            // Repository sẽ bắt và ném lại ConcurrencyException → middleware trả về HTTP 409.
            else if (exception is ConcurrencyException concurrencyEx)
            {
                statusCode = HttpStatusCode.Conflict;
                response = BaseResponse<object>.Fail(
                    concurrencyEx.ErrorCode,
                    exception.Message
                );
            }
            else if (exception is UnauthorizedAccessException unauthorizedEx)
            {
                statusCode = HttpStatusCode.Unauthorized;
                response = BaseResponse<object>.Fail(
                    "UNAUTHORIZED",
                    unauthorizedEx.Message
                );
            }
            else if (exception is AppException appEx)
            {
                statusCode = HttpStatusCode.InternalServerError;
                response = BaseResponse<object>.Fail(
                    appEx.ErrorCode,
                    exception.Message
                );
            }
            else
            {
                // For any other unexpected exception type
                statusCode = HttpStatusCode.InternalServerError;
                response = BaseResponse<object>.Fail(
                    "INTERNAL_SERVER_ERROR",
                    "An unexpected error occurred. Please contact support."
                );
            }

            context.Response.StatusCode = (int)statusCode;

            // Serialize and write the response
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);

            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
