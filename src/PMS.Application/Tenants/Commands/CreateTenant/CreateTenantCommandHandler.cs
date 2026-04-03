using MediatR;
using PMS.Application.Common.Interfaces;
using PMS.Domain.Tenants;

namespace PMS.Application.Tenants.Commands.CreateTenant;

public sealed class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Guid>
{
    private readonly ITenantRepository _tenants;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTenantCommandHandler(ITenantRepository tenants, IUnitOfWork unitOfWork)
    {
        _tenants = tenants;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = Tenant.Create(request.Name);
        await _tenants.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return tenant.Id;
    }
}
