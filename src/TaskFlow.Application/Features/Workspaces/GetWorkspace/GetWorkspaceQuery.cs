using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Workspaces.GetWorkspace
{
    public record GetWorkspaceQuery(Guid WorkspaceId) : IWorkspaceScoped, IRequest<Workspace>;
}
