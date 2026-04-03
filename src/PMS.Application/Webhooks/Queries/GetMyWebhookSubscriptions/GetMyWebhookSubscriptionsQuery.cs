using PMS.Application.CQRS.Abstractions;

namespace PMS.Application.Webhooks.Queries.GetMyWebhookSubscriptions;

public sealed record GetMyWebhookSubscriptionsQuery : IQuery<IReadOnlyList<WebhookSubscriptionDto>>;
