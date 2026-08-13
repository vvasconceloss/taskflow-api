using FluentAssertions;
using Moq;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Auth.Register;
using TaskFlow.Domain.Entities;

namespace TaskFlow.UnitTests.Handlers.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();

    [Fact]
    public async Task Handle_WhenEmailIsAvailable_ShouldCreateAndReturnUser()
    {
        _passwordHasher.Setup(h => h.Hash("StrongPass123")).Returns("hashed-password");
        var handler = new RegisterCommandHandler(_users.Object, _passwordHasher.Object);
        var command = new RegisterCommand("Victor", "victor@example.com", "StrongPass123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Email.Should().Be("victor@example.com");
        result.Name.Should().Be("Victor");
        result.PasswordHash.Should().Be("hashed-password");
        _users.Verify(
            u => u.AddAsync(It.Is<User>(x => x.Email == "victor@example.com"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailIsAlreadyTaken_ShouldThrowConflictAndNotPersist()
    {
        var existing = new User { Email = "victor@example.com", PasswordHash = "x" };
        _users.Setup(u => u.GetByEmailAsync("victor@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        var handler = new RegisterCommandHandler(_users.Object, _passwordHasher.Object);
        var command = new RegisterCommand(null, "victor@example.com", "StrongPass123");

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        _users.Verify(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
