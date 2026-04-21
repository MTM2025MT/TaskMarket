using Microsoft.EntityFrameworkCore;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Repositories;

public sealed class TaskMediaRepository : ITaskMediaRepository
{
    private readonly ApplicationDbContext _db;

    public TaskMediaRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<TaskMedia?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _db.Set<TaskMedia>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TaskMedia>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Set<TaskMedia>().ToListAsync(cancellationToken);

    public async Task AddAsync(TaskMedia media, CancellationToken cancellationToken = default)
        => await _db.Set<TaskMedia>().AddAsync(media, cancellationToken);

    public void Update(TaskMedia media)
        => _db.Set<TaskMedia>().Update(media);

    public void Remove(TaskMedia media)
        => _db.Set<TaskMedia>().Remove(media);
}