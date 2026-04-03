using FluentAssertions;
using Moq;
using PMS.Application.Common.IntegrationEvents;
using PMS.Application.Common.Interfaces;
using PMS.Application.LeaseContracts.Commands.CreateLeaseContract;
using PMS.Domain.LeaseContracts;

namespace PMS.Tests.Application.LeaseContracts;

public sealed class CreateLeaseContractCommandHandlerTests
{
    [Fact]
    public async Task Handle_when_tenant_context_missing_throws()
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(c => c.TenantId).Returns((Guid?)null);

        var handler = new CreateLeaseContractCommandHandler(
            new Mock<ILeaseContractRepository>().Object,
            new Mock<IUnitOfWork>().Object,
            new Mock<IIntegrationEventPublisher>().Object,
            currentUser.Object);

        var act = () => handler.Handle(
            new CreateLeaseContractCommand(
                "Lease",
                new DateOnly(2026, 1, 1),
                new DateOnly(2027, 1, 1),
                100m),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Tenant context is required.");
    }

    [Fact]
    public async Task Handle_persists_lease_publishes_event_and_returns_id()
    {
        var tenantId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(c => c.TenantId).Returns(tenantId);

        var leases = new Mock<ILeaseContractRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var events = new Mock<IIntegrationEventPublisher>();

        var handler = new CreateLeaseContractCommandHandler(
            leases.Object,
            unitOfWork.Object,
            events.Object,
            currentUser.Object);

        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2027, 1, 1);
        var id = await handler.Handle(
            new CreateLeaseContractCommand("Main lease", start, end, 1500m, "eur"),
            CancellationToken.None);

        id.Should().NotBeEmpty();
        leases.Verify(
            x => x.AddAsync(It.Is<LeaseContract>(l =>
                    l.TenantId == tenantId &&
                    l.Title == "Main lease" &&
                    l.StartDate == start &&
                    l.EndDate == end &&
                    l.MonthlyRent == 1500m &&
                    l.Currency == "EUR"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        events.Verify(
            x => x.PublishAsync(
                It.Is<LeaseContractCreatedIntegrationEvent>(e =>
                    e.LeaseContractId == id &&
                    e.TenantId == tenantId &&
                    e.Title == "Main lease" &&
                    e.OccurredAtUtc <= DateTime.UtcNow),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
