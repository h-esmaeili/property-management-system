using PMS.Infrastructure;
using PMS.Worker;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "PMS.Worker");
});

builder.Services.Configure<WebhookDeliveryRetryOptions>(builder.Configuration.GetSection(WebhookDeliveryRetryOptions.SectionName));
builder.Services.AddInfrastructureForWorker(builder.Configuration);
builder.Services.AddHttpClient("webhooks", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHostedService<RabbitMqWebhookDeliveryConsumer>();

var host = builder.Build();

try
{
    host.Run();
}
finally
{
    Log.CloseAndFlush();
}
