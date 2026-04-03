using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Common.IntegrationEvents;
using PMS.Application.Webhooks.Commands.CreateWebhookSubscription;
using PMS.Application.Webhooks.Queries.GetMyWebhookSubscriptions;
using PMS.Api.Models.Webhooks;

namespace PMS.Api.Controllers;

/// <summary>Manage outbound webhook subscriptions for the current user (JWT required).</summary>
[ApiController]
[Route("api/v1/webhook-subscriptions")]
[Authorize]
[Tags("Webhooks")]
public sealed class WebhookSubscriptionsController : ControllerBase
{
    private readonly ISender _sender;

    public WebhookSubscriptionsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>List webhook URLs for the current user, grouped by event type.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WebhookSubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetMyWebhookSubscriptionsQuery(), cancellationToken);
        return Ok(items);
    }

    /// <summary>Register a webhook URL for the current user for the given integration event.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateWebhookSubscriptionRequest body, CancellationToken cancellationToken)
    {
        var eventType = string.IsNullOrWhiteSpace(body.EventType)
            ? IntegrationEventNames.LeaseContractCreated
            : body.EventType.Trim();

        var id = await _sender.Send(
            new CreateWebhookSubscriptionCommand(body.Url, eventType, body.Secret),
            cancellationToken);
        return Created($"/api/v1/webhook-subscriptions/{id}", new { id });
    }
}
