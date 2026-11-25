using TaskMarket.Models;

namespace TaskMarket.IRepositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Review>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Review>> GetByTargetIdAsync(int targetId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int taskId, int reviewerId, CancellationToken cancellationToken = default);
    Task<List<int>> GetRatingsForTargetAsync(int targetId, CancellationToken cancellationToken = default);
    Task AddAsync(Review review, CancellationToken cancellationToken = default);
    void Update(Review review);
    void Remove(Review review);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}