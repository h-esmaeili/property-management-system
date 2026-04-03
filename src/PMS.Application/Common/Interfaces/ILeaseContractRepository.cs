using PMS.Domain.LeaseContracts;

namespace PMS.Application.Common.Interfaces;

public interface ILeaseContractRepository
{
    Task AddAsync(LeaseContract leaseContract, CancellationToken cancellationToken = default);
}
