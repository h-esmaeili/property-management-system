namespace PMS.Api.Models.Auth;

public sealed record RegisterUserRequest(string Email, string Password, string OrganizationName);
