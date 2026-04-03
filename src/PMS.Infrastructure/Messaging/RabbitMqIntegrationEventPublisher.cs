using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PMS.Application.Common.Interfaces;
using RabbitMQ.Client;

namespace PMS.Infrastructure.Messaging;

public sealed class RabbitMqIntegrationEventPublisher : IIntegrationEventPublisher, IDisposable
{
    private readonly ILogger<RabbitMqIntegrationEventPublisher> _logger;
    private readonly RabbitMqSettings _settings;
    private readonly Lazy<IConnection> _connection;

    public RabbitMqIntegrationEventPublisher(
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqIntegrationEventPublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _connection = new Lazy<IConnection>(CreateConnection);
    }

    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : class
    {
        var conn = _connection.Value;
        using var channel = conn.CreateModel();

        channel.ExchangeDeclare(
            exchange: _settings.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        var typeName = typeof(T).Name;
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(integrationEvent));
        var props = channel.CreateBasicProperties();
        props.ContentType = "application/json";
        props.Type = typeName;

        channel.BasicPublish(
            exchange: _settings.ExchangeName,
            routingKey: typeName,
            basicProperties: props,
            body: body);

        _logger.LogInformation("Published integration event {EventType}", typeName);
        return Task.CompletedTask;
    }

    private IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = string.IsNullOrWhiteSpace(_settings.VirtualHost) ? "/" : _settings.VirtualHost,
            DispatchConsumersAsync = true
        };

        return factory.CreateConnection();
    }

    public void Dispose()
    {
        if (_connection.IsValueCreated)
            _connection.Value.Dispose();
    }
}
