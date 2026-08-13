using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Common.Behaviors
{
    public class WorkspaceAuthorizationBehavior<TRequest, TResponse>(
        IWorkspaceRepository workspaces,
        ICurrentUserService currentUser)
        : IPipelineBehavior<TRequest, TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (request is IWorkspaceScoped scoped)
            {
                if (!await workspaces.IsMemberAsync(scoped.WorkspaceId, currentUser.UserId, cancellationToken))
                {
                    throw new ForbiddenException("You are not a member of this workspace.");
                }

                if (request is IAdminWorkspaceScoped &&
                    !await workspaces.IsAdminAsync(scoped.WorkspaceId, currentUser.UserId, cancellationToken))
                {
                    throw new ForbiddenException("Admin role is required for this operation.");
                }
            }

            return await next();
        }
    }
}
