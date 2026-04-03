using MediatR;
using Microsoft.AspNetCore.Identity;
using PMS.Application.Common.Interfaces;
using PMS.Domain.Users;

namespace PMS.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUserRepository _users;
    private readonly IJwtTokenProvider _jwt;
    private readonly IPasswordHasher<User> _passwordHasher;

    public LoginCommandHandler(
        IUserRepository users,
        IJwtTokenProvider jwt,
        IPasswordHasher<User> passwordHasher)
    {
        _users = users;
        _jwt = jwt;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var auth = await _users.GetByEmailForAuthenticationAsync(email, cancellationToken);
        if (auth is null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var user = auth.User;
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var jwt = _jwt.CreateToken(user, auth.RoleName, cancellationToken);
        return new LoginResponseDto(jwt.AccessToken, jwt.ExpiresAtUtc, user.Id, user.TenantId);
    }
}
