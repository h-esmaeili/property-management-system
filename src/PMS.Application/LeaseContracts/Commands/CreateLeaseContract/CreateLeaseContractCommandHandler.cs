using MediatR;
using PMS.Application.Common.IntegrationEvents;
using PMS.Application.Common.Interfaces;
using PMS.Domain.LeaseContracts;

namespace PMS.Application.LeaseContracts.Commands.CreateLeaseContract;

public sealed class CreateLeaseContractCommandHandler : IRequestHandler<CreateLeaseContractCommand, Guid>
{
    private readonly ILeaseContractRepository _leases;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _events;
    private readonly ICurrentUserService _currentUser;

    public CreateLeaseContractCommandHandler(
        ILeaseContractRepository leases,
        IUnitOfWork unitOfWork,
        IIntegrationEventPublisher events,
        ICurrentUserService currentUser)
    {
        _leases = leases;
        _unitOfWork = unitOfWork;
        _events = events;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateLeaseContractCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant context is required.");

        var lease = LeaseContract.Create(
            tenantId,
            request.Title,
            request.StartDate,
            request.EndDate,
            request.MonthlyRent,
            request.Currency);

        await _leases.AddAsync(lease, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _events.PublishAsync(
            new LeaseContractCreatedIntegrationEvent(lease.Id, lease.TenantId, lease.Title, DateTime.UtcNow),
            cancellationToken);

        return lease.Id;
    }
}
