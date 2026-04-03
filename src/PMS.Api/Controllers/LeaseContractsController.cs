using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.LeaseContracts.Commands.CreateLeaseContract;
using PMS.Api.Models.LeaseContracts;

namespace PMS.Api.Controllers;

[ApiController]
[Route("api/v1/lease-contracts")]
[Authorize]
[Tags("Lease contracts")]
public sealed class LeaseContractsController : ControllerBase
{
    private readonly ISender _sender;

    public LeaseContractsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
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
        return Created($"/api/v1/lease-contracts/{id}", new { id });
    }
}
