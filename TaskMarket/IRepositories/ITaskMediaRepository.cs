using TaskMarket.Models;

namespace TaskMarket.IRepositories;

public interface ITaskMediaRepository
{
    Task<TaskMedia?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskMedia>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TaskMedia media, CancellationToken cancellationToken = default);
    void Update(TaskMedia media);
    void Remove(TaskMedia media);
}