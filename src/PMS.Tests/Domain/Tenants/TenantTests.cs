using FluentAssertions;
using PMS.Domain.Tenants;

namespace PMS.Tests.Domain.Tenants;

public sealed class TenantTests
{
    [Fact]
    public void Create_with_valid_name_sets_trimmed_name()
    {
        var tenant = Tenant.Create("  Acme Corp  ");

        tenant.Name.Should().Be("Acme Corp");
        tenant.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_empty_name_throws(string name)
    {
        var act = () => Tenant.Create(name);

        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }
}
