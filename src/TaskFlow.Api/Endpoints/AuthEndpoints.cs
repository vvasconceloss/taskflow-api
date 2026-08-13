using MediatR;
using TaskFlow.Api.Contracts;
using TaskFlow.Api.Extensions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Auth.GetMe;
using TaskFlow.Application.Features.Auth.Login;
using TaskFlow.Application.Features.Auth.Register;

namespace TaskFlow.Api.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/auth");

            group.MapPost("/register", async (RegisterCommand command, ISender sender) =>
            {
                var user = await sender.Send(command);
                return Results.Created(string.Empty, new UserResponse(user.Id, user.Email, user.Name));
            }).WithSummary("Registers a new user")
              .Produces<UserResponse>(StatusCodes.Status201Created)
              .WithApiErrorResponses();

            group.MapPost("/login", async (LoginCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(result);
            }).RequireRateLimiting("login")
              .WithSummary("Authenticates and returns a JWT token")
              .Produces<TokenResult>(StatusCodes.Status200OK)
              .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
              .Produces<ApiErrorResponse>(StatusCodes.Status429TooManyRequests);

            group.MapGet("/me", async (ISender sender) =>
            {
                var user = await sender.Send(new GetMeQuery());
                return Results.Ok(new UserResponse(user.Id, user.Email, user.Name));
            }).RequireAuthorization()
              .WithSummary("Returns the authenticated user")
              .Produces<UserResponse>(StatusCodes.Status200OK)
              .WithApiErrorResponses();

            return app;
        }
    }
}
