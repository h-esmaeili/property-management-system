using PMS.Domain.Common;

namespace PMS.Domain.Tenants;

public sealed class Tenant : Entity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;

    private Tenant()
    {
    }

    public static Tenant Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name.Trim()
        };
    }
}
