using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Workspaces.UpdateWorkspace
{
    public record UpdateWorkspaceCommand(Guid WorkspaceId, string Name) : IAdminWorkspaceScoped, IRequest<Workspace>;
}
