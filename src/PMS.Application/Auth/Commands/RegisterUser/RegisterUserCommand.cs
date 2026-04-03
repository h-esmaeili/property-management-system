using PMS.Application.CQRS.Abstractions;

namespace PMS.Application.Auth.Commands.RegisterUser;

/// <summary>Creates an organization (tenant) and the first user account.</summary>
public sealed record RegisterUserCommand(string Email, string Password, string OrganizationName) : ICommand<Guid>;
