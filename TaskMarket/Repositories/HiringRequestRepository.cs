using Microsoft.EntityFrameworkCore;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Repositories;

public sealed class HiringRequestRepository : IHiringRequestRepository
{
    private readonly ApplicationDbContext _db;

    public HiringRequestRepository(ApplicationDbContext db)
    {
        _db = db;
    }
     
    public async Task<HiringRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _db.HiringRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<List<HiringRequest>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.HiringRequests.ToListAsync(cancellationToken);

    public async Task<int> AddAsync(HiringRequest request, CancellationToken cancellationToken = default)
    {
        await _db.HiringRequests.AddAsync(request, cancellationToken);
         
        return await SaveChangesAsync(cancellationToken);
    }
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int rowEffected = await _db.SaveChangesAsync(cancellationToken);
        return rowEffected;
    }
    public async Task<int> SaveChangesAsync()
    {
        int rowEffected = await _db.SaveChangesAsync();
        return rowEffected;
    }

    public Task<int> Update(HiringRequest request, CancellationToken cancellationToken = default)
    {
       _db.HiringRequests.Update(request);
        return SaveChangesAsync();
    }

    public Task<int> Remove(HiringRequest request, CancellationToken cancellationToken = default)
    {
        _db.HiringRequests.Remove(request);
        return SaveChangesAsync();
    }
}