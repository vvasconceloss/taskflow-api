using Microsoft.AspNetCore.Diagnostics;
using TaskFlow.Application.Common.Exceptions;

namespace TaskFlow.Api.Exceptions
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, message) = exception switch
            {
                ConflictException => (StatusCodes.Status409Conflict, exception.Message),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, exception.Message),
                ForbiddenException => (StatusCodes.Status403Forbidden, exception.Message),
                NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                ValidationException => (StatusCodes.Status400BadRequest, exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Unhandled exception");
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(new { message }, cancellationToken);

            return true;
        }
    }
}
