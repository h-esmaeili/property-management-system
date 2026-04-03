using PMS.Application.CQRS.Abstractions;

namespace PMS.Application.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<LoginResponseDto>;
