using Microsoft.EntityFrameworkCore;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Repositories;

public sealed class ServiceCategoryRepository : IServiceCategoryRepository
{
    private readonly ApplicationDbContext _db;

    public ServiceCategoryRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _db.ServiceCategories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<List<ServiceCategory>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.ServiceCategories.ToListAsync(cancellationToken);

    public async Task AddAsync(ServiceCategory category, CancellationToken cancellationToken = default)
        => await _db.ServiceCategories.AddAsync(category, cancellationToken);

    public void Update(ServiceCategory category)
        => _db.ServiceCategories.Update(category);

    public void Remove(ServiceCategory category)
        => _db.ServiceCategories.Remove(category);
}