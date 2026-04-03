namespace PMS.Application.Common.Interfaces;

public interface IRoleRepository
{
    Task<Guid?> GetIdByNameAsync(string name, CancellationToken cancellationToken = default);
}
