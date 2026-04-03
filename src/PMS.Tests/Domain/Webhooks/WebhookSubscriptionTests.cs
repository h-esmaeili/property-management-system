using FluentAssertions;
using PMS.Domain.Webhooks;

namespace PMS.Tests.Domain.Webhooks;

public sealed class WebhookSubscriptionTests
{
    [Theory]
    [InlineData("https://api.example.com/hooks")]
    [InlineData("http://localhost:8080/callback")]
    public void Create_with_valid_args_sets_properties_and_trims(string url)
    {
        var userId = Guid.NewGuid();

        var sub = WebhookSubscription.Create(
            userId,
            $"  {url}  ",
            "  LeaseSigned  ",
            "  secret  ");

        sub.UserId.Should().Be(userId);
        sub.Url.Should().Be(url.Trim());
        sub.EventType.Should().Be("LeaseSigned");
        sub.Secret.Should().Be("secret");
        sub.IsActive.Should().BeTrue();
        sub.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_with_null_or_whitespace_secret_sets_secret_null()
    {
        WebhookSubscription.Create(Guid.NewGuid(), "https://x.test/", "evt", null)
            .Secret.Should().BeNull();
        WebhookSubscription.Create(Guid.NewGuid(), "https://x.test/", "evt", "   ")
            .Secret.Should().BeNull();
    }

    [Fact]
    public void Create_with_empty_user_id_throws()
    {
        var act = () => WebhookSubscription.Create(Guid.Empty, "https://x.test/", "evt", null);

        act.Should().Throw<ArgumentException>().WithParameterName("userId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_empty_url_throws(string url)
    {
        var act = () => WebhookSubscription.Create(Guid.NewGuid(), url, "evt", null);

        act.Should().Throw<ArgumentException>().WithParameterName("url");
    }

    [Theory]
    [InlineData("/relative/path")]
    [InlineData("not-a-uri")]
    [InlineData("ftp://example.com/hook")]
    public void Create_with_invalid_url_throws(string url)
    {
        var act = () => WebhookSubscription.Create(Guid.NewGuid(), url, "evt", null);

        act.Should().Throw<ArgumentException>().WithParameterName("url");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_empty_event_type_throws(string eventType)
    {
        var act = () => WebhookSubscription.Create(Guid.NewGuid(), "https://x.test/", eventType, null);

        act.Should().Throw<ArgumentException>().WithParameterName("eventType");
    }
}
