using TaskMarket.Models;

namespace TaskMarket.IRepositories;

public interface IOfferApplicationRepository
{
    Task<OfferApplication?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<OfferApplication>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(OfferApplication application, CancellationToken cancellationToken = default);
    void Update(OfferApplication application);
    void Remove(OfferApplication application);
}