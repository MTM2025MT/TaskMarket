using Microsoft.EntityFrameworkCore;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Repositories;

public sealed class OfferApplicationRepository : IOfferApplicationRepository
{
    private readonly ApplicationDbContext _db;

    public OfferApplicationRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<OfferApplication?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _db.OfferApplications.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<OfferApplication>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.OfferApplications.ToListAsync(cancellationToken);

    public async Task AddAsync(OfferApplication application, CancellationToken cancellationToken = default)
        => await _db.OfferApplications.AddAsync(application, cancellationToken);

    public void Update(OfferApplication application)
        => _db.OfferApplications.Update(application);

    public void Remove(OfferApplication application)
        => _db.OfferApplications.Remove(application);
}