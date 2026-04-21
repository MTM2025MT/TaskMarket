using TaskMarket.Models;

namespace TaskMarket.IRepositories;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Message>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Message message, CancellationToken cancellationToken = default);
    void Update(Message message);
    void Remove(Message message);
}