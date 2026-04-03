using PMS.Domain.Common;

namespace PMS.Domain.Webhooks;

public sealed class WebhookSubscription : Entity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    /// <summary>Matches integration event type name (e.g. routing key in RabbitMQ).</summary>
    public string EventType { get; private set; } = string.Empty;
    public string? Secret { get; private set; }
    public bool IsActive { get; private set; }

    private WebhookSubscription()
    {
    }

    public static WebhookSubscription Create(Guid userId, string url, string eventType, string? secret)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Url is required.", nameof(url));
        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            throw new ArgumentException("Url must be an absolute http or https URL.", nameof(url));
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type is required.", nameof(eventType));

        return new WebhookSubscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Url = url.Trim(),
            EventType = eventType.Trim(),
            Secret = string.IsNullOrWhiteSpace(secret) ? null : secret.Trim(),
            IsActive = true
        };
    }
}
