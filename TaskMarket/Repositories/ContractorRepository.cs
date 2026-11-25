using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Repositories;

public sealed class ContractorRepository : IContractorRepository
{
    private readonly ApplicationDbContext _db;

    public ContractorRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Contractor?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _db.Contractors
            .Include(c => c.ContractorServiceCategories)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<List<Contractor>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Contractors.ToListAsync(cancellationToken);

    public async Task AddAsync(Contractor contractor, CancellationToken cancellationToken = default)
        => await _db.Contractors.AddAsync(contractor, cancellationToken);

    public void Update(Contractor contractor)
        => _db.Contractors.Update(contractor);

    public void Remove(Contractor contractor)
        => _db.Contractors.Remove(contractor);

    // Return the top `count` contractors by RatingAvg (highest first).
    // Ordering then Take is the correct and efficient approach for this.
    public async Task<List<Contractor>> GetBestAsync(int count = 5, CancellationToken cancellationToken = default)
        => await _db.Contractors
            .OrderByDescending(c => c.RatingAvg)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<bool> BusinessNameExistsAsync(string businessName, CancellationToken cancellationToken = default)
        => _db.Contractors.AnyAsync(c => c.BusinessName == businessName, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
