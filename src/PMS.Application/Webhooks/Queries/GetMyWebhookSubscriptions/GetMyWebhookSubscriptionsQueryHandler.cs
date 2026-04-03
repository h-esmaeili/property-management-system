using MediatR;
using PMS.Application.Common.Interfaces;

namespace PMS.Application.Webhooks.Queries.GetMyWebhookSubscriptions;

public sealed class GetMyWebhookSubscriptionsQueryHandler
    : IRequestHandler<GetMyWebhookSubscriptionsQuery, IReadOnlyList<WebhookSubscriptionDto>>
{
    private readonly IWebhookSubscriptionRepository _webhooks;
    private readonly ICurrentUserService _currentUser;

    public GetMyWebhookSubscriptionsQueryHandler(
        IWebhookSubscriptionRepository webhooks,
        ICurrentUserService currentUser)
    {
        _webhooks = webhooks;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<WebhookSubscriptionDto>> Handle(
        GetMyWebhookSubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User context is required.");

        var items = await _webhooks.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        return items
            .Select(x => new WebhookSubscriptionDto(
                x.Id,
                x.Url,
                x.EventType,
                x.IsActive,
                !string.IsNullOrEmpty(x.Secret)))
            .ToList();
    }
}
