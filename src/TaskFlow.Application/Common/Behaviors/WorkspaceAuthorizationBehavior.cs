using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Common.Behaviors
{
    public class WorkspaceAuthorizationBehavior<TRequest, TResponse>(
        IWorkspaceRepository workspaces,
        IProjectRepository projects,
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
                await EnsureMemberAsync(scoped.WorkspaceId, cancellationToken);

                if (request is IAdminWorkspaceScoped &&
                    !await workspaces.IsAdminAsync(scoped.WorkspaceId, currentUser.UserId, cancellationToken))
                {
                    throw new ForbiddenException("Admin role is required for this operation.");
                }
            }

            if (request is IProjectScoped projectScoped)
            {
                var workspaceId = await projects.GetWorkspaceIdByProjectIdAsync(projectScoped.ProjectId, cancellationToken)
                    ?? throw new NotFoundException("Project not found.");

                await EnsureMemberAsync(workspaceId, cancellationToken);
            }

            return await next();
        }

        private async Task EnsureMemberAsync(Guid workspaceId, CancellationToken cancellationToken)
        {
            if (!await workspaces.IsMemberAsync(workspaceId, currentUser.UserId, cancellationToken))
            {
                throw new ForbiddenException("You are not a member of this workspace.");
            }
        }
    }
}
