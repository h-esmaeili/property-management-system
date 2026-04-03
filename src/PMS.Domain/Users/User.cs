using PMS.Domain.Common;

namespace PMS.Domain.Users;

public sealed class User : Entity, IAggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    private User()
    {
    }

    public static User Create(Guid tenantId, string email, Guid roleId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant is required.", nameof(tenantId));
        if (roleId == Guid.Empty)
            throw new ArgumentException("Role is required.", nameof(roleId));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        return new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RoleId = roleId,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = string.Empty
        };
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        PasswordHash = passwordHash;
    }
}
