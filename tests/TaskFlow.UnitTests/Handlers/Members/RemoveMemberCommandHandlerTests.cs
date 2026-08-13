using FluentAssertions;
using Moq;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Members.RemoveMember;
using TaskFlow.Domain.Entities;

namespace TaskFlow.UnitTests.Handlers.Members;

public class RemoveMemberCommandHandlerTests
{
    private readonly Mock<IWorkspaceRepository> _workspaces = new();

    private static WorkspaceMember AdminMember(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        WorkspaceId = Guid.NewGuid(),
        UserId = userId,
        Role = WorkspaceRole.Admin
    };

    [Fact]
    public async Task Handle_WhenRemovingLastAdmin_ShouldThrowConflict()
    {
        var member = AdminMember(Guid.NewGuid());
        _workspaces.Setup(w => w.GetMembershipAsync(It.IsAny<Guid>(), member.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);
        _workspaces.Setup(w => w.CountAdminsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        var handler = new RemoveMemberCommandHandler(_workspaces.Object);

        var act = () => handler.Handle(
            new RemoveMemberCommand(member.WorkspaceId, member.UserId), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        _workspaces.Verify(w => w.RemoveMemberAsync(It.IsAny<WorkspaceMember>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRemovingNonLastAdmin_ShouldRemove()
    {
        var member = AdminMember(Guid.NewGuid());
        _workspaces.Setup(w => w.GetMembershipAsync(It.IsAny<Guid>(), member.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);
        _workspaces.Setup(w => w.CountAdminsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        var handler = new RemoveMemberCommandHandler(_workspaces.Object);

        var result = await handler.Handle(
            new RemoveMemberCommand(member.WorkspaceId, member.UserId), CancellationToken.None);

        result.Should().Be(MediatR.Unit.Value);
        _workspaces.Verify(w => w.RemoveMemberAsync(member, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMembershipNotFound_ShouldThrowNotFound()
    {
        _workspaces.Setup(w => w.GetMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkspaceMember?)null);
        var handler = new RemoveMemberCommandHandler(_workspaces.Object);

        var act = () => handler.Handle(
            new RemoveMemberCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
