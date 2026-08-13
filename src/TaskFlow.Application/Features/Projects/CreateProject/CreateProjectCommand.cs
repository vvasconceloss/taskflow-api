using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Projects.CreateProject
{
    public record CreateProjectCommand(Guid WorkspaceId, string Name, string? Description)
        : IWorkspaceScoped, IRequest<Project>;
}
