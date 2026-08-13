using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Projects.ListProjects
{
    public record ListProjectsQuery(Guid WorkspaceId) : IWorkspaceScoped, IRequest<List<Project>>;
}
