namespace PMS.Application.Webhooks.Queries.GetMyWebhookSubscriptions;

public sealed record WebhookSubscriptionDto(
    Guid Id,
    string Url,
    string EventType,
    bool IsActive,
    bool HasSecret);
