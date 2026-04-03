using FluentAssertions;
using PMS.Domain.LeaseContracts;

namespace PMS.Tests.Domain.LeaseContracts;

public sealed class LeaseContractTests
{
    [Fact]
    public void Create_with_valid_args_sets_properties()
    {
        var tenantId = Guid.NewGuid();
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2027, 1, 1);

        var lease = LeaseContract.Create(
            tenantId,
            "  Main St Unit 1  ",
            start,
            end,
            1500m,
            "  eur  ");

        lease.TenantId.Should().Be(tenantId);
        lease.Title.Should().Be("Main St Unit 1");
        lease.StartDate.Should().Be(start);
        lease.EndDate.Should().Be(end);
        lease.MonthlyRent.Should().Be(1500m);
        lease.Currency.Should().Be("EUR");
        lease.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_with_whitespace_currency_defaults_to_usd()
    {
        var lease = LeaseContract.Create(
            Guid.NewGuid(),
            "Title",
            new DateOnly(2026, 1, 1),
            new DateOnly(2027, 1, 1),
            0m,
            "   ");

        lease.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_with_empty_tenant_id_throws()
    {
        var act = () => LeaseContract.Create(
            Guid.Empty,
            "Title",
            new DateOnly(2026, 1, 1),
            new DateOnly(2027, 1, 1),
            0m);

        act.Should().Throw<ArgumentException>().WithParameterName("tenantId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_empty_title_throws(string title)
    {
        var act = () => LeaseContract.Create(
            Guid.NewGuid(),
            title,
            new DateOnly(2026, 1, 1),
            new DateOnly(2027, 1, 1),
            0m);

        act.Should().Throw<ArgumentException>().WithParameterName("title");
    }

    [Fact]
    public void Create_when_end_not_after_start_throws()
    {
        var start = new DateOnly(2026, 6, 1);

        var actSameDay = () => LeaseContract.Create(
            Guid.NewGuid(),
            "Title",
            start,
            start,
            0m);

        actSameDay.Should().Throw<ArgumentException>()
            .WithMessage("End date must be after start date.*");

        var actBefore = () => LeaseContract.Create(
            Guid.NewGuid(),
            "Title",
            start,
            start.AddDays(-1),
            0m);

        actBefore.Should().Throw<ArgumentException>()
            .WithMessage("End date must be after start date.*");
    }

    [Fact]
    public void Create_with_negative_monthly_rent_throws()
    {
        var act = () => LeaseContract.Create(
            Guid.NewGuid(),
            "Title",
            new DateOnly(2026, 1, 1),
            new DateOnly(2027, 1, 1),
            -1m);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("monthlyRent");
    }
}
