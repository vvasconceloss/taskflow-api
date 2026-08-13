using MediatR;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Workspaces.CreateWorkspace
{
    public record CreateWorkspaceCommand(string Name) : IRequest<Workspace>;
}
