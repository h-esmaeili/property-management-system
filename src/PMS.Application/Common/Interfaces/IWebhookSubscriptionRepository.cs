using PMS.Domain.Webhooks;

namespace PMS.Application.Common.Interfaces;

public interface IWebhookSubscriptionRepository
{
    Task<IReadOnlyList<WebhookSubscription>> GetActiveByTenantAndEventAsync(
        Guid tenantId,
        string eventType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WebhookSubscription>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(WebhookSubscription subscription, CancellationToken cancellationToken = default);
}
