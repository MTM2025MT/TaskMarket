using TaskMarket.Models;

namespace TaskMarket.IRepositories;

public interface IServiceCategoryRepository
{
    Task<ServiceCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<ServiceCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ServiceCategory category, CancellationToken cancellationToken = default);
    void Update(ServiceCategory category);
    void Remove(ServiceCategory category);
}