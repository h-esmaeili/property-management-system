using MediatR;
using PMS.Application.Common.Interfaces;

namespace PMS.Application.Tenants.Queries.GetTenantById;

public sealed class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantDto?>
{
    private readonly ITenantRepository _tenants;

    public GetTenantByIdQueryHandler(ITenantRepository tenants)
    {
        _tenants = tenants;
    }

    public async Task<TenantDto?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenants.GetByIdAsync(request.Id, cancellationToken);
        return tenant is null ? null : new TenantDto(tenant.Id, tenant.Name);
    }
}
