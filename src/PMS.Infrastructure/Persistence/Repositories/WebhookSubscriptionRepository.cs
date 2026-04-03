using Microsoft.EntityFrameworkCore;
using PMS.Application.Common.Interfaces;
using PMS.Domain.Webhooks;

namespace PMS.Infrastructure.Persistence.Repositories;

public sealed class WebhookSubscriptionRepository : IWebhookSubscriptionRepository
{
    private readonly AppDbContext _db;

    public WebhookSubscriptionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<WebhookSubscription>> GetActiveByTenantAndEventAsync(
        Guid tenantId,
        string eventType,
        CancellationToken cancellationToken = default)
    {
        return await (
                from ws in _db.WebhookSubscriptions.AsNoTracking()
                join u in _db.Users.AsNoTracking() on ws.UserId equals u.Id
                where u.TenantId == tenantId && ws.EventType == eventType && ws.IsActive
                select ws)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WebhookSubscription>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _db.WebhookSubscriptions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.EventType)
            .ThenBy(x => x.Url)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WebhookSubscription subscription, CancellationToken cancellationToken = default) =>
        await _db.WebhookSubscriptions.AddAsync(subscription, cancellationToken);
}
