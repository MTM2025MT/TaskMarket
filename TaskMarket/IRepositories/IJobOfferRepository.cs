using TaskMarket.Models;

namespace TaskMarket.IRepositories;

public interface IJobOfferRepository
{
    Task<JobOffer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<JobOffer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(JobOffer offer, CancellationToken cancellationToken = default);
    void Update(JobOffer offer);
    void Remove(JobOffer offer);
}