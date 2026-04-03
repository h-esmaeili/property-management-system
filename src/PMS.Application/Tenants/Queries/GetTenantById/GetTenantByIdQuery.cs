using PMS.Application.CQRS.Abstractions;

namespace PMS.Application.Tenants.Queries.GetTenantById;

public sealed record GetTenantByIdQuery(Guid Id) : IQuery<TenantDto?>;
