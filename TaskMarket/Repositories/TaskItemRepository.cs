
using Microsoft.EntityFrameworkCore;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Repositories;

public sealed class TaskItemRepository : ITaskItemRepository
{
    private readonly ApplicationDbContext _db;

    public TaskItemRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _db.Tasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<List<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Tasks.ToListAsync(cancellationToken);

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
        => await _db.Tasks.AddAsync(task, cancellationToken);

    public void Update(TaskItem task)
        => _db.Tasks.Update(task);

    public void Remove(TaskItem task)
        => _db.Tasks.Remove(task);
}