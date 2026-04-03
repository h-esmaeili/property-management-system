using PMS.Application.Common;
using PMS.Domain.Users;

namespace PMS.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserForAuthentication?> GetByEmailForAuthenticationAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
