using Microsoft.EntityFrameworkCore;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Repositories;

public sealed class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _db;

    public ReviewRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Review?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _db.Reviews.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<List<Review>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Reviews.ToListAsync(cancellationToken);

    public async Task<List<Review>> GetByTargetIdAsync(int targetId, CancellationToken cancellationToken = default)
        => await _db.Reviews
            .Where(x => x.TargetId == targetId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(int taskId, int reviewerId, CancellationToken cancellationToken = default)
        => _db.Reviews.AnyAsync(x => x.TaskId == taskId && x.ReviewerId == reviewerId, cancellationToken);

    public async Task<List<int>> GetRatingsForTargetAsync(int targetId, CancellationToken cancellationToken = default)
        => await _db.Reviews
            .Where(x => x.TargetId == targetId)
            .Select(x => x.Rating)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Review review, CancellationToken cancellationToken = default)
        => await _db.Reviews.AddAsync(review, cancellationToken);

    public void Update(Review review)
        => _db.Reviews.Update(review);

    public void Remove(Review review)
        => _db.Reviews.Remove(review);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}