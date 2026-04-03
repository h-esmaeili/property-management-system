using MediatR;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Auth.Commands.Login;
using PMS.Application.Auth.Commands.RegisterUser;
using PMS.Api.Models.Auth;

namespace PMS.Api.Controllers;

/// <summary>Register a user (creates tenant + account) and log in to obtain a JWT.</summary>
[ApiController]
[Route("api/v1/auth")]
[Tags("Auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ISender sender, ILogger<AuthController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>Create an organization and the first user.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = await _sender.Send(
                new RegisterUserCommand(request.Email, request.Password, request.OrganizationName),
                cancellationToken);
            _logger.LogInformation("Registration succeeded for user {UserId}", userId);
            return Created($"/api/v1/users/{userId}", new { id = userId });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Registration rejected: {Reason}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Authenticate and receive a JWT for subsequent requests.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
            _logger.LogInformation(
                "Login succeeded for user {UserId} in tenant {TenantId}",
                result.UserId,
                result.TenantId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Login failed (invalid credentials)");
            return Unauthorized();
        }
    }
}
