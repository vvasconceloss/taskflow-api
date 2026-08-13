using MediatR;
using TaskFlow.Api.Contracts;
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
            });

            group.MapPost("/login", async (LoginCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(result);
            });

            group.MapGet("/me", async (ISender sender) =>
            {
                var user = await sender.Send(new GetMeQuery());
                return Results.Ok(new UserResponse(user.Id, user.Email, user.Name));
            }).RequireAuthorization();

            return app;
        }
    }
}
