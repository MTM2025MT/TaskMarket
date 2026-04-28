using Microsoft.EntityFrameworkCore;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Repositories;

public sealed class JobOfferRepository : IJobOfferRepository
{
    private readonly ApplicationDbContext _db;

    public JobOfferRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<JobOffer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _db.JobOffers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<List<JobOffer>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.JobOffers.ToListAsync(cancellationToken);

    public async Task AddAsync(JobOffer offer, CancellationToken cancellationToken = default)
        => await _db.JobOffers.AddAsync(offer, cancellationToken);

    public void Update(JobOffer offer)
        => _db.JobOffers.Update(offer);

    public void Remove(JobOffer offer)
        => _db.JobOffers.Remove(offer);
}