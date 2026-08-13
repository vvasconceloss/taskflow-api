using FluentAssertions;
using Moq;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Auth.Login;
using TaskFlow.Domain.Entities;

namespace TaskFlow.UnitTests.Handlers.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenService> _tokenService = new();

    private User CreateUser() => new() { Id = Guid.NewGuid(), Email = "victor@example.com", PasswordHash = "hash" };

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnToken()
    {
        var user = CreateUser();
        _users.Setup(u => u.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("StrongPass123", user.PasswordHash)).Returns(true);
        var expected = new TokenResult("jwt-token", DateTime.UtcNow.AddMinutes(60));
        _tokenService.Setup(t => t.CreateToken(user)).Returns(expected);
        var handler = new LoginCommandHandler(_users.Object, _passwordHasher.Object, _tokenService.Object);

        var result = await handler.Handle(new LoginCommand(user.Email, "StrongPass123"), CancellationToken.None);

        result.Should().Be(expected);
        _tokenService.Verify(t => t.CreateToken(user), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowUnauthorized()
    {
        _users.Setup(u => u.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var handler = new LoginCommandHandler(_users.Object, _passwordHasher.Object, _tokenService.Object);

        var act = () => handler.Handle(new LoginCommand("ghost@example.com", "StrongPass123"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _tokenService.Verify(t => t.CreateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ShouldThrowUnauthorized()
    {
        var user = CreateUser();
        _users.Setup(u => u.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify(It.IsAny<string>(), user.PasswordHash)).Returns(false);
        var handler = new LoginCommandHandler(_users.Object, _passwordHasher.Object, _tokenService.Object);

        var act = () => handler.Handle(new LoginCommand(user.Email, "WrongPass123"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
