using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PMS.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        const int slowMs = 2000;
        var name = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next();
            sw.Stop();
            if (sw.ElapsedMilliseconds >= slowMs)
            {
                _logger.LogWarning(
                    "MediatR {Request} completed slowly in {ElapsedMs}ms",
                    name,
                    sw.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogDebug("MediatR {Request} completed in {ElapsedMs}ms", name, sw.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "MediatR {Request} failed after {ElapsedMs}ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
