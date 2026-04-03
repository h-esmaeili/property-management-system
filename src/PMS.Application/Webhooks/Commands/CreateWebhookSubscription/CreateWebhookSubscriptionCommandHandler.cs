using MediatR;
using PMS.Application.Common.Interfaces;
using PMS.Domain.Webhooks;

namespace PMS.Application.Webhooks.Commands.CreateWebhookSubscription;

public sealed class CreateWebhookSubscriptionCommandHandler
    : IRequestHandler<CreateWebhookSubscriptionCommand, Guid>
{
    private readonly IWebhookSubscriptionRepository _webhooks;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateWebhookSubscriptionCommandHandler(
        IWebhookSubscriptionRepository webhooks,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _webhooks = webhooks;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateWebhookSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User context is required.");

        var subscription = WebhookSubscription.Create(userId, request.Url, request.EventType, request.Secret);
        await _webhooks.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return subscription.Id;
    }
}
