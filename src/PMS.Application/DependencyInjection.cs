using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PMS.Application.Common.Behaviors;

namespace PMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        return services;
    }
}
