using TaskMarket.Models;

namespace TaskMarket.IRepositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Payment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Payment>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    void Update(Payment payment);
    void Remove(Payment payment);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}