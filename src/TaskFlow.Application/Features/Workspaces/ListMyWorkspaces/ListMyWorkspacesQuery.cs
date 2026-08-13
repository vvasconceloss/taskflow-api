using MediatR;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Workspaces.ListMyWorkspaces
{
    public record ListMyWorkspacesQuery : IRequest<List<Workspace>>;
}
