using PMS.Domain.Common;

namespace PMS.Domain.LeaseContracts;

public sealed class LeaseContract : Entity, IAggregateRoot
{
    public Guid TenantId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public decimal MonthlyRent { get; private set; }
    public string Currency { get; private set; } = "USD";

    private LeaseContract()
    {
    }

    public static LeaseContract Create(
        Guid tenantId,
        string title,
        DateOnly startDate,
        DateOnly endDate,
        decimal monthlyRent,
        string currency = "USD")
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.");
        if (monthlyRent < 0)
            throw new ArgumentOutOfRangeException(nameof(monthlyRent));

        return new LeaseContract
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Title = title.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            MonthlyRent = monthlyRent,
            Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant()
        };
    }
}
