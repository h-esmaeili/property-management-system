using PMS.Application.CQRS.Abstractions;

namespace PMS.Application.LeaseContracts.Commands.CreateLeaseContract;

public sealed record CreateLeaseContractCommand(
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal MonthlyRent,
    string Currency = "USD") : ICommand<Guid>;
