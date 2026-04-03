namespace PMS.Application.Auth.Commands.Login;

public sealed record LoginResponseDto(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid UserId,
    Guid TenantId);
