using PMS.Application.CQRS.Abstractions;

namespace PMS.Application.Webhooks.Commands.CreateWebhookSubscription;

public sealed record CreateWebhookSubscriptionCommand(string Url, string EventType, string? Secret) : ICommand<Guid>;
