using MediatR;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Features.Projects.DeleteProject
{
    public record DeleteProjectCommand(Guid ProjectId) : IProjectScoped, IRequest<Unit>;
}
