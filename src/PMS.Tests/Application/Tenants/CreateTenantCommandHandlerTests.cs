using FluentAssertions;
using Moq;
using PMS.Application.Common.Interfaces;
using PMS.Application.Tenants.Commands.CreateTenant;
using PMS.Domain.Tenants;

namespace PMS.Tests.Application.Tenants;

public sealed class CreateTenantCommandHandlerTests
{
    [Fact]
    public async Task Handle_persists_tenant_and_returns_id()
    {
        var tenants = new Mock<ITenantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateTenantCommandHandler(tenants.Object, unitOfWork.Object);

        var id = await handler.Handle(new CreateTenantCommand("Acme"), CancellationToken.None);

        id.Should().NotBeEmpty();
        tenants.Verify(
            x => x.AddAsync(It.Is<Tenant>(t => t.Name == "Acme"), It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
