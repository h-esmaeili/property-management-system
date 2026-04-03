using MediatR;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Auth.Commands.Login;
using PMS.Application.Auth.Commands.RegisterUser;
using PMS.Api.Models.Auth;

namespace PMS.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Tags("Auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

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
            return Created($"/api/v1/users/{userId}", new { id = userId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }
}
