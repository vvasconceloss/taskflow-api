using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Projects.ArchiveProject
{
    public record ArchiveProjectCommand(Guid ProjectId) : IProjectScoped, IRequest<Project>;
}
