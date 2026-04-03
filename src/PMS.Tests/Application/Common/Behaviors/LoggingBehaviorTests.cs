using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using PMS.Application.Common.Behaviors;

namespace PMS.Tests.Application.Common.Behaviors;

public sealed class LoggingBehaviorTests
{
    private sealed record EchoRequest : IRequest<int>;

    [Fact]
    public async Task Handle_returns_next_result()
    {
        var behavior = new LoggingBehavior<EchoRequest, int>(
            NullLogger<LoggingBehavior<EchoRequest, int>>.Instance);

        var result = await behavior.Handle(
            new EchoRequest(),
            () => Task.FromResult(42),
            CancellationToken.None);

        result.Should().Be(42);
    }

    [Fact]
    public async Task Handle_propagates_exception_from_next()
    {
        var behavior = new LoggingBehavior<EchoRequest, int>(
            NullLogger<LoggingBehavior<EchoRequest, int>>.Instance);

        var act = () => behavior.Handle(
            new EchoRequest(),
            () => Task.FromException<int>(new InvalidOperationException("boom")),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }
}
