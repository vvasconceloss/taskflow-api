using FluentAssertions;
using Moq;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Application.Features.Workspaces.ListMyWorkspaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.UnitTests.Handlers.Workspaces;

public class ListMyWorkspacesQueryHandlerTests
{
    private readonly Mock<IWorkspaceRepository> _workspaces = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    [Fact]
    public async Task Handle_WithInvalidSortField_ShouldThrowValidation()
    {
        _workspaces.Setup(w => w.GetWorkspacesForUserAsync(It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Workspace>(new List<Workspace>(), 1, 20, 0, 0));
        var handler = new ListMyWorkspacesQueryHandler(_workspaces.Object, _currentUser.Object);

        var act = () => handler.Handle(
            new ListMyWorkspacesQuery(1, 20, "id", false), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
