using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using PMS.Application.Auth.Commands.Login;
using PMS.Application.Common.Interfaces;
using PMS.Domain.Users;

namespace PMS.Tests.Application.Auth;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_when_user_not_found_throws()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(u => u.GetByEmailAsync("a@b.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new LoginCommandHandler(
            users.Object,
            new Mock<IJwtTokenProvider>().Object,
            new Mock<IPasswordHasher<User>>().Object);

        var act = () => handler.Handle(
            new LoginCommand("  A@B.COM  ", "pw"),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_when_password_invalid_throws()
    {
        var user = User.Create(Guid.NewGuid(), "a@b.com");
        user.SetPasswordHash("stored");

        var users = new Mock<IUserRepository>();
        users.Setup(u => u.GetByEmailAsync("a@b.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var hasher = new Mock<IPasswordHasher<User>>();
        hasher.Setup(h => h.VerifyHashedPassword(user, "stored", "wrong"))
            .Returns(PasswordVerificationResult.Failed);

        var handler = new LoginCommandHandler(
            users.Object,
            new Mock<IJwtTokenProvider>().Object,
            hasher.Object);

        var act = () => handler.Handle(new LoginCommand("a@b.com", "wrong"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_returns_token_and_ids_when_valid()
    {
        var tenantId = Guid.NewGuid();
        var user = User.Create(tenantId, "a@b.com");
        user.SetPasswordHash("stored");

        var users = new Mock<IUserRepository>();
        users.Setup(u => u.GetByEmailAsync("a@b.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var hasher = new Mock<IPasswordHasher<User>>();
        hasher.Setup(h => h.VerifyHashedPassword(user, "stored", "good"))
            .Returns(PasswordVerificationResult.Success);

        var expires = DateTime.UtcNow.AddHours(1);
        var jwt = new Mock<IJwtTokenProvider>();
        jwt.Setup(j => j.CreateToken(user, It.IsAny<CancellationToken>()))
            .Returns(new JwtTokenResult("token-123", expires));

        var handler = new LoginCommandHandler(users.Object, jwt.Object, hasher.Object);

        var result = await handler.Handle(new LoginCommand("a@b.com", "good"), CancellationToken.None);

        result.AccessToken.Should().Be("token-123");
        result.ExpiresAtUtc.Should().Be(expires);
        result.UserId.Should().Be(user.Id);
        result.TenantId.Should().Be(tenantId);
    }
}
