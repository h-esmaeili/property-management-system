using PMS.Infrastructure;
using PMS.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<WebhookDeliveryRetryOptions>(builder.Configuration.GetSection(WebhookDeliveryRetryOptions.SectionName));
builder.Services.AddInfrastructureForWorker(builder.Configuration);
builder.Services.AddHttpClient("webhooks", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHostedService<RabbitMqWebhookDeliveryConsumer>();

var host = builder.Build();
host.Run();
