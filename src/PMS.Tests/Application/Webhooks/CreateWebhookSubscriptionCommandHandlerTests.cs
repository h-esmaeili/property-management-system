using FluentAssertions;
using Moq;
using PMS.Application.Common.Interfaces;
using PMS.Application.Webhooks.Commands.CreateWebhookSubscription;
using PMS.Domain.Webhooks;

namespace PMS.Tests.Application.Webhooks;

public sealed class CreateWebhookSubscriptionCommandHandlerTests
{
    [Fact]
    public async Task Handle_when_user_context_missing_throws()
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(c => c.UserId).Returns((Guid?)null);

        var handler = new CreateWebhookSubscriptionCommandHandler(
            new Mock<IWebhookSubscriptionRepository>().Object,
            new Mock<IUnitOfWork>().Object,
            currentUser.Object);

        var act = () => handler.Handle(
            new CreateWebhookSubscriptionCommand("https://example.com/hook", "LeaseSigned", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User context is required.");
    }

    [Fact]
    public async Task Handle_persists_subscription_and_returns_id()
    {
        var userId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(c => c.UserId).Returns(userId);

        var webhooks = new Mock<IWebhookSubscriptionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateWebhookSubscriptionCommandHandler(
            webhooks.Object,
            unitOfWork.Object,
            currentUser.Object);

        var id = await handler.Handle(
            new CreateWebhookSubscriptionCommand("https://example.com/hook", "LeaseSigned", "secret"),
            CancellationToken.None);

        id.Should().NotBeEmpty();
        webhooks.Verify(
            x => x.AddAsync(It.Is<WebhookSubscription>(s =>
                    s.UserId == userId &&
                    s.Url == "https://example.com/hook" &&
                    s.EventType == "LeaseSigned" &&
                    s.Secret == "secret"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
