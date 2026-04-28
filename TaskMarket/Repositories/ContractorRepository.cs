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
        => await _db.Contractors.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<List<Contractor>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Contractors.ToListAsync(cancellationToken);

    public async Task AddAsync(Contractor contractor, CancellationToken cancellationToken = default)
        => await _db.Contractors.AddAsync(contractor, cancellationToken);

    public void Update(Contractor contractor)
        => _db.Contractors.Update(contractor);

    public void Remove(Contractor contractor)
        => _db.Contractors.Remove(contractor);
}
