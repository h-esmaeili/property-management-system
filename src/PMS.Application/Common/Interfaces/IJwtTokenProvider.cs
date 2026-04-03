using PMS.Domain.Users;

namespace PMS.Application.Common.Interfaces;

public sealed record JwtTokenResult(string AccessToken, DateTime ExpiresAtUtc);

public interface IJwtTokenProvider
{
    JwtTokenResult CreateToken(User user, string roleName, CancellationToken cancellationToken = default);
}
