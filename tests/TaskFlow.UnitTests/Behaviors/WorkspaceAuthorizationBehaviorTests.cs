using FluentAssertions;
using MediatR;
using Moq;
using TaskFlow.Application.Common.Behaviors;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.UnitTests.Behaviors;

public class WorkspaceAuthorizationBehaviorTests
{
    private readonly Mock<IWorkspaceRepository> _workspaces = new();
    private readonly Mock<IProjectRepository> _projects = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private sealed class ScopedRequest(Guid workspaceId) : IWorkspaceScoped, IRequest<Unit>
    {
        public Guid WorkspaceId { get; } = workspaceId;
    }

    private sealed class AdminScopedRequest(Guid workspaceId) : IAdminWorkspaceScoped, IRequest<Unit>
    {
        public Guid WorkspaceId { get; } = workspaceId;
    }

    private sealed class ProjectScopedRequest(Guid projectId) : IProjectScoped, IRequest<Unit>
    {
        public Guid ProjectId { get; } = projectId;
    }

    private WorkspaceAuthorizationBehavior<TRequest, Unit> CreateBehavior<TRequest>()
        where TRequest : IRequest<Unit>
        => new(_workspaces.Object, _projects.Object, _currentUser.Object);

    [Fact]
    public async Task Handle_WhenMember_ShouldContinueToNext()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);
        _workspaces.Setup(w => w.IsMemberAsync(It.IsAny<Guid>(), userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var behavior = CreateBehavior<ScopedRequest>();
        var nextCalled = false;

        await behavior.Handle(
            new ScopedRequest(Guid.NewGuid()),
            _ => { nextCalled = true; return Task.FromResult(Unit.Value); },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenNotMember_ShouldThrowForbidden()
    {
        _workspaces.Setup(w => w.IsMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var behavior = CreateBehavior<ScopedRequest>();

        var act = () => behavior.Handle(
            new ScopedRequest(Guid.NewGuid()),
            _ => Task.FromResult(Unit.Value),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenMemberButNotAdmin_OnAdminRequest_ShouldThrowForbidden()
    {
        _workspaces.Setup(w => w.IsMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _workspaces.Setup(w => w.IsAdminAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var behavior = CreateBehavior<AdminScopedRequest>();

        var act = () => behavior.Handle(
            new AdminScopedRequest(Guid.NewGuid()),
            _ => Task.FromResult(Unit.Value),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenAdmin_OnAdminRequest_ShouldContinueToNext()
    {
        _workspaces.Setup(w => w.IsMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _workspaces.Setup(w => w.IsAdminAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var behavior = CreateBehavior<AdminScopedRequest>();
        var nextCalled = false;

        await behavior.Handle(
            new AdminScopedRequest(Guid.NewGuid()),
            _ => { nextCalled = true; return Task.FromResult(Unit.Value); },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenMemberOfProjectsWorkspace_ShouldContinueToNext()
    {
        var workspaceId = Guid.NewGuid();
        _projects.Setup(p => p.GetWorkspaceIdByProjectIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspaceId);
        _workspaces.Setup(w => w.IsMemberAsync(workspaceId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var behavior = CreateBehavior<ProjectScopedRequest>();
        var nextCalled = false;

        await behavior.Handle(
            new ProjectScopedRequest(Guid.NewGuid()),
            _ => { nextCalled = true; return Task.FromResult(Unit.Value); },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenNotMemberOfProjectsWorkspace_ShouldThrowForbidden()
    {
        _projects.Setup(p => p.GetWorkspaceIdByProjectIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());
        _workspaces.Setup(w => w.IsMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var behavior = CreateBehavior<ProjectScopedRequest>();

        var act = () => behavior.Handle(
            new ProjectScopedRequest(Guid.NewGuid()),
            _ => Task.FromResult(Unit.Value),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenProjectDoesNotExist_ShouldThrowNotFound()
    {
        _projects.Setup(p => p.GetWorkspaceIdByProjectIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);
        var behavior = CreateBehavior<ProjectScopedRequest>();

        var act = () => behavior.Handle(
            new ProjectScopedRequest(Guid.NewGuid()),
            _ => Task.FromResult(Unit.Value),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
