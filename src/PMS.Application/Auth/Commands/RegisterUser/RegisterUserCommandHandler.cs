using MediatR;
using Microsoft.AspNetCore.Identity;
using PMS.Application.Common.Interfaces;
using PMS.Domain.Tenants;
using PMS.Domain.Users;

namespace PMS.Application.Auth.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly ITenantRepository _tenants;
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<User> _passwordHasher;

    public RegisterUserCommandHandler(
        ITenantRepository tenants,
        IUserRepository users,
        IRoleRepository roles,
        IUnitOfWork unitOfWork,
        IPasswordHasher<User> passwordHasher)
    {
        _tenants = tenants;
        _users = users;
        _roles = roles;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await _users.GetByEmailAsync(email, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException("A user with this email already exists.");

        var tenant = Tenant.Create(request.OrganizationName);
        await _tenants.AddAsync(tenant, cancellationToken);

        var ownerRoleId = await _roles.GetIdByNameAsync(RoleNames.Owner, cancellationToken);
        if (ownerRoleId is null)
            throw new InvalidOperationException("The Owner role is not configured.");

        var user = User.Create(tenant.Id, email, ownerRoleId.Value);
        var hash = _passwordHasher.HashPassword(user, request.Password);
        user.SetPasswordHash(hash);

        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
