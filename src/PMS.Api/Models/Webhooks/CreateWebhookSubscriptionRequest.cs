namespace PMS.Api.Models.Webhooks;

public sealed class CreateWebhookSubscriptionRequest
{
    public string Url { get; set; } = string.Empty;

    /// <summary>Optional; defaults to lease contract created event.</summary>
    public string? EventType { get; set; }

    public string? Secret { get; set; }
}
