namespace PMS.Worker;

public sealed class WebhookDeliveryRetryOptions
{
    public const string SectionName = "WebhookDelivery";

    /// <summary>Total attempts including the first try.</summary>
    public int MaxAttempts { get; set; } = 5;

    public int BaseDelayMilliseconds { get; set; } = 500;

    public int MaxDelayMilliseconds { get; set; } = 30_000;

    public bool UseJitter { get; set; } = true;
}
