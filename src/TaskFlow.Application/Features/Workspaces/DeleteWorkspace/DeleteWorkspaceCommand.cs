using MediatR;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Features.Workspaces.DeleteWorkspace
{
    public record DeleteWorkspaceCommand(Guid WorkspaceId) : IAdminWorkspaceScoped, IRequest<Unit>;
}
