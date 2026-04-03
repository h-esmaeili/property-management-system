using System.Security.Claims;
using Serilog;

namespace PMS.Api.Logging;

internal static class RequestLoggingExtensions
{
    public static WebApplication UsePmsSerilogRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);
                if (httpContext.User.Identity?.IsAuthenticated != true)
                    return;

                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var tenantId = httpContext.User.FindFirstValue("tenant_id");
                if (!string.IsNullOrEmpty(userId))
                    diagnosticContext.Set("UserId", userId);
                if (!string.IsNullOrEmpty(tenantId))
                    diagnosticContext.Set("TenantId", tenantId);
            };
        });

        return app;
    }
}
