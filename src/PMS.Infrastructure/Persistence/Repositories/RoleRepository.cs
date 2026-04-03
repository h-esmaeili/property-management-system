using Microsoft.EntityFrameworkCore;
using PMS.Application.Common.Interfaces;
using PMS.Domain.Users;

namespace PMS.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _db;

    public RoleRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Guid?> GetIdByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var normalized = name.Trim();
        return await _db.Roles.AsNoTracking()
            .Where(r => r.Name == normalized)
            .Select(r => (Guid?)r.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
