using TaskFlow.Api.Contracts;

namespace TaskFlow.Api.Extensions
{
    public static class OpenApiExtensions
    {
        public static RouteHandlerBuilder WithApiErrorResponses(this RouteHandlerBuilder builder) =>
            builder
                .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
                .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden)
                .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);
    }
}
