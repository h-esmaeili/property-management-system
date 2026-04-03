using PMS.Application.CQRS.Abstractions;

namespace PMS.Application.Tenants.Commands.CreateTenant;

public sealed record CreateTenantCommand(string Name) : ICommand<Guid>;
