using TaskMarket.Models;

namespace TaskMarket.IRepositories;

public interface IHiringRequestRepository
{
    Task<HiringRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<HiringRequest>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> AddAsync(HiringRequest request, CancellationToken cancellationToken = default);
    Task<int> Update(HiringRequest request, CancellationToken cancellationToken = default);
    Task<int> Remove(HiringRequest request, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync();
} 