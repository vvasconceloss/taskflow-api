using FluentAssertions;
using Moq;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Members.UpdateMemberRole;
using TaskFlow.Domain.Entities;

namespace TaskFlow.UnitTests.Handlers.Members;

public class UpdateMemberRoleCommandHandlerTests
{
    private readonly Mock<IWorkspaceRepository> _workspaces = new();

    [Fact]
    public async Task Handle_WhenDemotingLastAdmin_ShouldThrowConflict()
    {
        var member = new WorkspaceMember
        {
            Id = Guid.NewGuid(),
            WorkspaceId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Role = WorkspaceRole.Admin
        };
        _workspaces.Setup(w => w.GetMembershipAsync(It.IsAny<Guid>(), member.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);
        _workspaces.Setup(w => w.CountAdminsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        var handler = new UpdateMemberRoleCommandHandler(_workspaces.Object);

        var act = () => handler.Handle(
            new UpdateMemberRoleCommand(member.WorkspaceId, member.UserId, WorkspaceRole.Member),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        _workspaces.Verify(w => w.UpdateMemberAsync(It.IsAny<WorkspaceMember>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDemotingNonLastAdmin_ShouldUpdateRole()
    {
        var member = new WorkspaceMember
        {
            Id = Guid.NewGuid(),
            WorkspaceId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Role = WorkspaceRole.Admin
        };
        _workspaces.Setup(w => w.GetMembershipAsync(It.IsAny<Guid>(), member.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);
        _workspaces.Setup(w => w.CountAdminsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        var handler = new UpdateMemberRoleCommandHandler(_workspaces.Object);

        var result = await handler.Handle(
            new UpdateMemberRoleCommand(member.WorkspaceId, member.UserId, WorkspaceRole.Member),
            CancellationToken.None);

        result.Role.Should().Be(WorkspaceRole.Member);
        _workspaces.Verify(w => w.UpdateMemberAsync(member, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPromotingToAdmin_ShouldUpdateWithoutCheck()
    {
        var member = new WorkspaceMember
        {
            Id = Guid.NewGuid(),
            WorkspaceId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Role = WorkspaceRole.Member
        };
        _workspaces.Setup(w => w.GetMembershipAsync(It.IsAny<Guid>(), member.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);
        var handler = new UpdateMemberRoleCommandHandler(_workspaces.Object);

        var result = await handler.Handle(
            new UpdateMemberRoleCommand(member.WorkspaceId, member.UserId, WorkspaceRole.Admin),
            CancellationToken.None);

        result.Role.Should().Be(WorkspaceRole.Admin);
        _workspaces.Verify(w => w.CountAdminsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
