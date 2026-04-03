namespace PMS.Api.Models.LeaseContracts;

public sealed record CreateLeaseContractRequest(
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal MonthlyRent,
    string? Currency);
