using Microsoft.EntityFrameworkCore;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _db;

    public PaymentRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _db.Payments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<List<Payment>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Payments.ToListAsync(cancellationToken);

    public async Task<List<Payment>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => await _db.Payments
            .Where(x => x.PayerId == userId || x.PayeeId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
        => await _db.Payments.AddAsync(payment, cancellationToken);

    public void Update(Payment payment)
        => _db.Payments.Update(payment);

    public void Remove(Payment payment)
        => _db.Payments.Remove(payment);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}