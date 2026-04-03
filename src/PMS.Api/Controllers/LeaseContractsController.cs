using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.LeaseContracts.Commands.CreateLeaseContract;
using PMS.Api.Models.LeaseContracts;

namespace PMS.Api.Controllers;

/// <summary>Create and manage lease contracts for the current tenant (JWT required).</summary>
[ApiController]
[Route("api/v1/lease-contracts")]
[Authorize]
[Tags("Lease contracts")]
public sealed class LeaseContractsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<LeaseContractsController> _logger;

    public LeaseContractsController(ISender sender, ILogger<LeaseContractsController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>Create a lease contract and publish an integration event.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateLeaseContractRequest request, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(
            new CreateLeaseContractCommand(
                request.Title,
                request.StartDate,
                request.EndDate,
                request.MonthlyRent,
                request.Currency ?? "USD"),
            cancellationToken);

        Guid? tenantId = null;
        if (Guid.TryParse(User.FindFirstValue("tenant_id"), out var tid))
            tenantId = tid;

        _logger.LogInformation(
            "Created lease contract {LeaseContractId} for tenant {TenantId}",
            id,
            tenantId);

        return Created($"/api/v1/lease-contracts/{id}", new { id });
    }
}