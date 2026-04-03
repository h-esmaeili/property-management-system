using Microsoft.EntityFrameworkCore;
using PMS.Application.Common;
using PMS.Application.Common.Interfaces;
using PMS.Domain.Users;

namespace PMS.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    public async Task<UserForAuthentication?> GetByEmailForAuthenticationAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (user is null)
            return null;
        return new UserForAuthentication(user, user.Role.Name);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await _db.Users.AddAsync(user, cancellationToken);
}
