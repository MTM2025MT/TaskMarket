using Microsoft.EntityFrameworkCore;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Repositories;

public sealed class MessageRepository : IMessageRepository
{
    private readonly ApplicationDbContext _db;

    public MessageRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Message?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _db.Messages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Message>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Messages.ToListAsync(cancellationToken);

    public async Task AddAsync(Message message, CancellationToken cancellationToken = default)
        => await _db.Messages.AddAsync(message, cancellationToken);

    public void Update(Message message)
        => _db.Messages.Update(message);

    public void Remove(Message message)
        => _db.Messages.Remove(message);
}