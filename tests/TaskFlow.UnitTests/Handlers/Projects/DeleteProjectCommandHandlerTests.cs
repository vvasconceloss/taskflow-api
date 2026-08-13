using FluentAssertions;
using Moq;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Projects.DeleteProject;
using TaskFlow.Domain.Entities;

namespace TaskFlow.UnitTests.Handlers.Projects;

public class DeleteProjectCommandHandlerTests
{
    private readonly Mock<IProjectRepository> _projects = new();

    private static Project Project() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Project",
        WorkspaceId = Guid.NewGuid()
    };

    [Fact]
    public async Task Handle_WhenProjectHasTasks_ShouldThrowConflict()
    {
        var project = Project();
        _projects.Setup(p => p.GetByIdAsync(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        _projects.Setup(p => p.HasTasksAsync(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = new DeleteProjectCommandHandler(_projects.Object);

        var act = () => handler.Handle(new DeleteProjectCommand(project.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        _projects.Verify(p => p.RemoveAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProjectHasNoTasks_ShouldRemove()
    {
        var project = Project();
        _projects.Setup(p => p.GetByIdAsync(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        _projects.Setup(p => p.HasTasksAsync(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new DeleteProjectCommandHandler(_projects.Object);

        var result = await handler.Handle(new DeleteProjectCommand(project.Id), CancellationToken.None);

        result.Should().Be(MediatR.Unit.Value);
        _projects.Verify(p => p.RemoveAsync(project, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrowNotFound()
    {
        _projects.Setup(p => p.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);
        var handler = new DeleteProjectCommandHandler(_projects.Object);

        var act = () => handler.Handle(new DeleteProjectCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
