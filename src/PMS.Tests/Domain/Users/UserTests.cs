using FluentAssertions;
using PMS.Domain.Users;

namespace PMS.Tests.Domain.Users;

public sealed class UserTests
{
    [Fact]
    public void Create_with_valid_args_sets_tenant_id_and_normalized_email()
    {
        var tenantId = Guid.NewGuid();

        var user = User.Create(tenantId, "  User@Example.COM  ");

        user.TenantId.Should().Be(tenantId);
        user.Email.Should().Be("user@example.com");
        user.PasswordHash.Should().BeEmpty();
        user.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_with_empty_tenant_id_throws()
    {
        var act = () => User.Create(Guid.Empty, "a@b.com");

        act.Should().Throw<ArgumentException>().WithParameterName("tenantId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_empty_email_throws(string email)
    {
        var act = () => User.Create(Guid.NewGuid(), email);

        act.Should().Throw<ArgumentException>().WithParameterName("email");
    }

    [Fact]
    public void SetPasswordHash_with_valid_value_sets_hash()
    {
        var user = User.Create(Guid.NewGuid(), "a@b.com");

        user.SetPasswordHash("  hash  ");

        user.PasswordHash.Should().Be("  hash  ");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetPasswordHash_with_empty_throws(string hash)
    {
        var user = User.Create(Guid.NewGuid(), "a@b.com");

        var act = () => user.SetPasswordHash(hash);

        act.Should().Throw<ArgumentException>().WithParameterName("passwordHash");
    }
}
