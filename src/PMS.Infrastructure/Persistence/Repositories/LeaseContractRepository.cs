using PMS.Application.Common.Interfaces;
using PMS.Domain.LeaseContracts;

namespace PMS.Infrastructure.Persistence.Repositories;

public sealed class LeaseContractRepository : ILeaseContractRepository
{
    private readonly AppDbContext _db;

    public LeaseContractRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(LeaseContract leaseContract, CancellationToken cancellationToken = default) =>
        await _db.LeaseContracts.AddAsync(leaseContract, cancellationToken);
}
