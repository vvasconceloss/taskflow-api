using FluentAssertions;
using Moq;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Application.Features.Projects.ListProjects;
using TaskFlow.Domain.Entities;

namespace TaskFlow.UnitTests.Handlers.Projects;

public class ListProjectsQueryHandlerTests
{
    private readonly Mock<IProjectRepository> _projects = new();

    [Fact]
    public async Task Handle_WithInvalidSortField_ShouldThrowValidation()
    {
        _projects.Setup(p => p.GetProjectsAsync(It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Project>(new List<Project>(), 1, 20, 0, 0));
        var handler = new ListProjectsQueryHandler(_projects.Object);

        var act = () => handler.Handle(
            new ListProjectsQuery(Guid.NewGuid(), 1, 20, "description", false), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
