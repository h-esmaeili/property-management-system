using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using PMS.Application.Auth.Commands.RegisterUser;
using PMS.Application.Common.Interfaces;
using PMS.Domain.Tenants;
using PMS.Domain.Users;

namespace PMS.Tests.Application.Auth;

public sealed class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_when_email_already_registered_throws()
    {
        var existing = User.Create(Guid.NewGuid(), "taken@example.com");
        var users = new Mock<IUserRepository>();
        users.Setup(u => u.GetByEmailAsync("taken@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var handler = new RegisterUserCommandHandler(
            new Mock<ITenantRepository>().Object,
            users.Object,
            new Mock<IUnitOfWork>().Object,
            new Mock<IPasswordHasher<User>>().Object);

        var act = () => handler.Handle(
            new RegisterUserCommand("Taken@Example.COM", "pw", "Org"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A user with this email already exists.");
    }

    [Fact]
    public async Task Handle_creates_tenant_user_and_persists()
    {
        var tenants = new Mock<ITenantRepository>();
        var users = new Mock<IUserRepository>();
        users.Setup(u => u.GetByEmailAsync("new@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var hasher = new Mock<IPasswordHasher<User>>();
        hasher.Setup(h => h.HashPassword(It.IsAny<User>(), "secret"))
            .Returns("hashed-secret");

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new RegisterUserCommandHandler(
            tenants.Object,
            users.Object,
            unitOfWork.Object,
            hasher.Object);

        var id = await handler.Handle(
            new RegisterUserCommand("  New@Example.COM  ", "secret", "  Contoso  "),
            CancellationToken.None);

        id.Should().NotBeEmpty();
        tenants.Verify(
            x => x.AddAsync(It.Is<Tenant>(t => t.Name == "Contoso"), It.IsAny<CancellationToken>()),
            Times.Once);
        users.Verify(
            x => x.AddAsync(It.Is<User>(u =>
                    u.Email == "new@example.com" &&
                    u.PasswordHash == "hashed-secret"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        hasher.Verify(h => h.HashPassword(It.IsAny<User>(), "secret"), Times.Once);
    }
}
