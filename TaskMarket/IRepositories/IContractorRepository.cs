using TaskMarket.Models;

namespace TaskMarket.IRepositories;

public interface IContractorRepository
{
    Task<Contractor?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Contractor>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Contractor contractor, CancellationToken cancellationToken = default);
    void Update(Contractor contractor);
    void Remove(Contractor contractor);
}
