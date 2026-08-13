using Microsoft.AspNetCore.Diagnostics;
using TaskFlow.Api.Contracts;
using TaskFlow.Application.Common.Exceptions;

namespace TaskFlow.Api.Exceptions
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, type, message) = exception switch
            {
                ConflictException => (StatusCodes.Status409Conflict, "ConflictError", exception.Message),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "UnauthorizedError", exception.Message),
                ForbiddenException => (StatusCodes.Status403Forbidden, "ForbiddenError", exception.Message),
                NotFoundException => (StatusCodes.Status404NotFound, "NotFoundError", exception.Message),
                ValidationException => (StatusCodes.Status400BadRequest, "ValidationError", exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "InternalServerError", "An unexpected error occurred.")
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Unhandled exception");
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(new ApiErrorResponse(
                type,
                message,
                (exception as ValidationException)?.Errors,
                httpContext.TraceIdentifier), cancellationToken);

            return true;
        }
    }
}
