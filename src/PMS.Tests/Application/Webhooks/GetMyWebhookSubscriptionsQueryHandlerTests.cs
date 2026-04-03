using FluentAssertions;
using Moq;
using PMS.Application.Common.Interfaces;
using PMS.Application.Webhooks.Queries.GetMyWebhookSubscriptions;
using PMS.Domain.Webhooks;

namespace PMS.Tests.Application.Webhooks;

public sealed class GetMyWebhookSubscriptionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_when_user_context_missing_throws()
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(c => c.UserId).Returns((Guid?)null);

        var handler = new GetMyWebhookSubscriptionsQueryHandler(
            new Mock<IWebhookSubscriptionRepository>().Object,
            currentUser.Object);

        var act = () => handler.Handle(new GetMyWebhookSubscriptionsQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User context is required.");
    }

    [Fact]
    public async Task Handle_maps_subscriptions_to_dtos()
    {
        var userId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(c => c.UserId).Returns(userId);

        var withSecret = WebhookSubscription.Create(userId, "https://a.test/", "evt1", "s");
        var secretNull = WebhookSubscription.Create(userId, "https://b.test/", "evt2", null);

        var webhooks = new Mock<IWebhookSubscriptionRepository>();
        webhooks.Setup(w => w.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WebhookSubscription> { withSecret, secretNull });

        var handler = new GetMyWebhookSubscriptionsQueryHandler(webhooks.Object, currentUser.Object);

        var list = await handler.Handle(new GetMyWebhookSubscriptionsQuery(), CancellationToken.None);

        list.Should().HaveCount(2);
        list[0].Should().BeEquivalentTo(new WebhookSubscriptionDto(
            withSecret.Id,
            withSecret.Url,
            withSecret.EventType,
            withSecret.IsActive,
            HasSecret: true));
        list[1].HasSecret.Should().BeFalse();
    }
}
