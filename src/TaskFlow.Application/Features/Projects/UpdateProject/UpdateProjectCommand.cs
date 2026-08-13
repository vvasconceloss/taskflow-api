using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Projects.UpdateProject
{
    public record UpdateProjectCommand(Guid ProjectId, string Name, string? Description)
        : IProjectScoped, IRequest<Project>;
}
