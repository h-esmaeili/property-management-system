namespace PMS.Application.Common.IntegrationEvents;

public sealed record LeaseContractCreatedIntegrationEvent(
    Guid LeaseContractId,
    Guid TenantId,
    string Title,
    DateTime OccurredAtUtc);
