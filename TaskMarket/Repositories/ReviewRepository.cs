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

    public async Task<IReadOnlyList<Review>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Reviews.ToListAsync(cancellationToken);

    public async Task AddAsync(Review review, CancellationToken cancellationToken = default)
        => await _db.Reviews.AddAsync(review, cancellationToken);

    public void Update(Review review)
        => _db.Reviews.Update(review);

    public void Remove(Review review)
        => _db.Reviews.Remove(review);
}