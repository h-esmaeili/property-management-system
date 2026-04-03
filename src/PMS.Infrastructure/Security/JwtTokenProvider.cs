using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PMS.Application.Common.Interfaces;
using PMS.Domain.Users;

namespace PMS.Infrastructure.Security;

public sealed class JwtTokenProvider : IJwtTokenProvider
{
    private readonly JwtSettings _settings;

    public JwtTokenProvider(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public JwtTokenResult CreateToken(User user, string roleName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name is required.", nameof(roleName));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(_settings.ExpiresHours);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("tenant_id", user.TenantId.ToString()),
            new(ClaimTypes.Role, roleName.Trim())
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var encoded = new JwtSecurityTokenHandler().WriteToken(token);
        return new JwtTokenResult(encoded, expires);
    }
}
