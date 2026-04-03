using FluentAssertions;
using Moq;
using PMS.Application.Common.Interfaces;
using PMS.Application.Tenants.Queries.GetTenantById;
using PMS.Domain.Tenants;

namespace PMS.Tests.Application.Tenants;

public sealed class GetTenantByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_when_tenant_missing_returns_null()
    {
        var tenants = new Mock<ITenantRepository>();
        var id = Guid.NewGuid();
        tenants.Setup(t => t.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Tenant?)null);

        var handler = new GetTenantByIdQueryHandler(tenants.Object);

        var dto = await handler.Handle(new GetTenantByIdQuery(id), CancellationToken.None);

        dto.Should().BeNull();
    }

    [Fact]
    public async Task Handle_when_tenant_exists_returns_dto()
    {
        var tenant = Tenant.Create("Acme");
        var tenants = new Mock<ITenantRepository>();
        tenants.Setup(t => t.GetByIdAsync(tenant.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        var handler = new GetTenantByIdQueryHandler(tenants.Object);

        var dto = await handler.Handle(new GetTenantByIdQuery(tenant.Id), CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(tenant.Id);
        dto.Name.Should().Be("Acme");
    }
}
