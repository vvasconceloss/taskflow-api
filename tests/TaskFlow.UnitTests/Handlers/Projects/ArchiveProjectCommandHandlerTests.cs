using FluentAssertions;
using Moq;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Projects.ArchiveProject;
using TaskFlow.Domain.Entities;

namespace TaskFlow.UnitTests.Handlers.Projects;

public class ArchiveProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldSetIsArchived()
    {
        var project = new Project { Id = Guid.NewGuid(), Name = "Project", WorkspaceId = Guid.NewGuid() };
        var projects = new Mock<IProjectRepository>();
        projects.Setup(p => p.GetByIdAsync(project.Id, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        var handler = new ArchiveProjectCommandHandler(projects.Object);

        var result = await handler.Handle(new ArchiveProjectCommand(project.Id), CancellationToken.None);

        result.IsArchived.Should().BeTrue();
        projects.Verify(p => p.UpdateAsync(project, It.IsAny<CancellationToken>()), Times.Once);
    }
}
