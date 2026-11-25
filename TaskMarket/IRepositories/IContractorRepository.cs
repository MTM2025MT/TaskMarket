using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskMarket.Models;

namespace TaskMarket.IRepositories;

public interface IContractorRepository
{
    Task<Contractor?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Contractor>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Contractor contractor, CancellationToken cancellationToken = default);
    void Update(Contractor contractor);
    void Remove(Contractor contractor);
    Task<List<Contractor>> GetBestAsync(int count = 5, CancellationToken cancellationToken = default);
    Task<bool> BusinessNameExistsAsync(string businessName, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
