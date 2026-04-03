using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PMS.Application.Common.IntegrationEvents;
using PMS.Application.Common.Interfaces;
using PMS.Domain.Webhooks;
using PMS.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PMS.Worker;

/// <summary>
/// Subscribes to the integration exchange and POSTs payloads to tenant webhook URLs.
/// </summary>
public sealed class RabbitMqWebhookDeliveryConsumer : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<RabbitMqSettings> _rabbitOptions;
    private readonly IOptions<WebhookDeliveryRetryOptions> _retryOptions;
    private readonly ILogger<RabbitMqWebhookDeliveryConsumer> _logger;

    public RabbitMqWebhookDeliveryConsumer(
        IHttpClientFactory httpClientFactory,
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqSettings> rabbitOptions,
        IOptions<WebhookDeliveryRetryOptions> retryOptions,
        ILogger<RabbitMqWebhookDeliveryConsumer> logger)
    {
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
        _rabbitOptions = rabbitOptions;
        _retryOptions = retryOptions;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = _rabbitOptions.Value;
        // Required for AsyncEventingBasicConsumer: async handlers + BasicAck after await run on the correct dispatcher.
        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            VirtualHost = string.IsNullOrWhiteSpace(settings.VirtualHost) ? "/" : settings.VirtualHost,
            DispatchConsumersAsync = true
        };

        using var connection = await ConnectWithRetryAsync(factory, stoppingToken).ConfigureAwait(false);
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: settings.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        const string queueName = "pms.webhook.delivery";
        channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queueName, settings.ExchangeName, routingKey: IntegrationEventNames.LeaseContractCreated);
        channel.BasicQos(0, prefetchCount: 10, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var routingKey = ea.RoutingKey ?? string.Empty;
                var bodyBytes = ea.Body.ToArray();
                _logger.LogDebug(
                    "Received message routingKey={RoutingKey} bytes={Length}",
                    routingKey,
                    bodyBytes.Length);
                var json = Encoding.UTF8.GetString(bodyBytes);

                if (routingKey == IntegrationEventNames.LeaseContractCreated)
                {
                    var evt = JsonSerializer.Deserialize<LeaseContractCreatedIntegrationEvent>(
                        json,
                        JsonOptions);
                    if (evt is not null)
                        await DeliverLeaseContractCreatedAsync(evt, stoppingToken).ConfigureAwait(false);
                }
                else
                    _logger.LogWarning("Unhandled routing key {RoutingKey}", routingKey);

                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook delivery pipeline failed; message discarded");
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        channel.BasicConsume(queueName, autoAck: false, consumer: consumer);
        _logger.LogInformation(
            "Webhook consumer listening on queue {Queue} for routing key {Key}",
            queueName,
            IntegrationEventNames.LeaseContractCreated);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
    }

    private async Task<IConnection> ConnectWithRetryAsync(ConnectionFactory factory, CancellationToken stoppingToken)
    {
        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var connection = factory.CreateConnection();
                _logger.LogInformation(
                    "Connected to RabbitMQ at {Host}:{Port}{AttemptNote}",
                    factory.HostName,
                    factory.Port,
                    attempt > 1 ? $" (attempt {attempt})" : string.Empty);
                return connection;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(
                    ex,
                    "RabbitMQ connection attempt {Attempt}/{Max} failed; retrying in 3s",
                    attempt,
                    maxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken).ConfigureAwait(false);
            }
        }

        throw new InvalidOperationException("Could not connect to RabbitMQ.");
    }

    private async Task DeliverLeaseContractCreatedAsync(LeaseContractCreatedIntegrationEvent evt, CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IWebhookSubscriptionRepository>();

        var subscriptions = await repo
            .GetActiveByTenantAndEventAsync(evt.TenantId, IntegrationEventNames.LeaseContractCreated, ct)
            .ConfigureAwait(false);

        if (subscriptions.Count == 0)
        {
            _logger.LogDebug("No webhook subscriptions for tenant {TenantId}", evt.TenantId);
            return;
        }

        var client = _httpClientFactory.CreateClient("webhooks");
        var payloadJson = JsonSerializer.Serialize(evt);
        var bodyBytes = Encoding.UTF8.GetBytes(payloadJson);
        var retry = _retryOptions.Value;

        foreach (var sub in subscriptions)
        {
            var deliveryId = Guid.NewGuid();
            await PostWebhookWithRetriesAsync(client, sub, evt, bodyBytes, deliveryId, retry, ct).ConfigureAwait(false);
        }
    }

    private async Task PostWebhookWithRetriesAsync(
        HttpClient client,
        WebhookSubscription sub,
        LeaseContractCreatedIntegrationEvent evt,
        byte[] bodyBytes,
        Guid deliveryId,
        WebhookDeliveryRetryOptions options,
        CancellationToken ct)
    {
        var max = Math.Max(1, options.MaxAttempts);

        for (var attempt = 1; attempt <= max; attempt++)
        {
            using var request = BuildHttpRequest(sub, evt, bodyBytes, deliveryId);
            try
            {
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct)
                    .ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    if (attempt > 1)
                    {
                        _logger.LogInformation(
                            "Webhook {Url} succeeded on attempt {Attempt}",
                            sub.Url,
                            attempt);
                    }

                    return;
                }

                var status = (int)response.StatusCode;
                if (!IsRetryableStatusCode(response.StatusCode))
                {
                    _logger.LogWarning(
                        "Webhook {Url} returned {Status} (non-retryable)",
                        sub.Url,
                        status);
                    return;
                }

                if (attempt >= max)
                {
                    _logger.LogError(
                        "Webhook {Url} failed after {Max} attempts with HTTP {Status}",
                        sub.Url,
                        max,
                        status);
                    return;
                }

                var delay = await ComputeDelayAfterHttpFailureAsync(response, attempt, options, ct).ConfigureAwait(false);
                _logger.LogWarning(
                    "Webhook {Url} attempt {Attempt}/{Max} returned {Status}; retrying after {DelayMs}ms",
                    sub.Url,
                    attempt,
                    max,
                    status,
                    delay.TotalMilliseconds);
                await Task.Delay(delay, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (IsRetryableTransportException(ex))
            {
                if (attempt >= max)
                {
                    _logger.LogError(
                        ex,
                        "Webhook {Url} failed after {Max} attempts",
                        sub.Url,
                        max);
                    return;
                }

                var delay = ComputeExponentialBackoffDelay(attempt, options);
                _logger.LogWarning(
                    ex,
                    "Webhook {Url} attempt {Attempt}/{Max} failed; retrying after {DelayMs}ms",
                    sub.Url,
                    attempt,
                    max,
                    delay.TotalMilliseconds);
                await Task.Delay(delay, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook {Url} failed with non-retryable error", sub.Url);
                return;
            }
        }
    }

    private static HttpRequestMessage BuildHttpRequest(
        WebhookSubscription sub,
        LeaseContractCreatedIntegrationEvent evt,
        byte[] bodyBytes,
        Guid deliveryId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, sub.Url);
        request.Content = new ByteArrayContent(bodyBytes);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request.Headers.TryAddWithoutValidation("X-Webhook-Event-Id", evt.LeaseContractId.ToString());
        request.Headers.TryAddWithoutValidation("X-Webhook-Event-Type", IntegrationEventNames.LeaseContractCreated);
        request.Headers.TryAddWithoutValidation("X-Webhook-Delivery-Id", deliveryId.ToString());

        if (!string.IsNullOrEmpty(sub.Secret))
        {
            var signature = ComputeHmacSha256Hex(sub.Secret, bodyBytes);
            request.Headers.TryAddWithoutValidation("X-Webhook-Signature", $"sha256={signature}");
        }

        return request;
    }

    private static bool IsRetryableStatusCode(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.NotImplemented
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    private static bool IsRetryableTransportException(Exception ex) =>
        ex is HttpRequestException or TaskCanceledException;

    private async Task<TimeSpan> ComputeDelayAfterHttpFailureAsync(
        HttpResponseMessage response,
        int attempt,
        WebhookDeliveryRetryOptions options,
        CancellationToken ct)
    {
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            if (response.Headers.RetryAfter?.Delta is { } delta)
                return CapDelay(delta, options);

            if (response.Headers.RetryAfter?.Date is { } date)
            {
                var until = date - DateTimeOffset.UtcNow;
                if (until > TimeSpan.Zero)
                    return CapDelay(until, options);
            }
        }

        await Task.CompletedTask.ConfigureAwait(false);
        return ComputeExponentialBackoffDelay(attempt, options);
    }

    private TimeSpan ComputeExponentialBackoffDelay(int attempt, WebhookDeliveryRetryOptions options)
    {
        var cap = Math.Max(1, options.MaxDelayMilliseconds);
        var baseMs = Math.Max(1, options.BaseDelayMilliseconds);
        var exp = Math.Min(cap, baseMs * Math.Pow(2, attempt - 1));
        var delayMs = exp;

        if (options.UseJitter)
        {
            var jitterRange = exp * 0.25;
            delayMs += Random.Shared.NextDouble() * jitterRange;
        }

        return CapDelay(TimeSpan.FromMilliseconds(delayMs), options);
    }

    private static TimeSpan CapDelay(TimeSpan delay, WebhookDeliveryRetryOptions options)
    {
        var max = TimeSpan.FromMilliseconds(Math.Max(1, options.MaxDelayMilliseconds));
        return delay > max ? max : delay;
    }

    private static string ComputeHmacSha256Hex(string secret, byte[] body)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(body);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
