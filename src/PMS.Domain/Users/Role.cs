using PMS.Domain.Common;

namespace PMS.Domain.Users;

public sealed class Role : Entity
{
    public string Name { get; private set; } = string.Empty;

    private Role()
    {
    }
}
