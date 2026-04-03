using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PMS.Application;

namespace PMS.Tests.Application;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_registers_IMediator()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IMediator>().Should().NotBeNull();
    }
}
