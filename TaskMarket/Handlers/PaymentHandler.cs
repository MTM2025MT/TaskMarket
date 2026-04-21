using ErrorOr;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class PaymentsHandler
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentsHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<ErrorOr<Success>> CreateIntentAsync(int taskId, decimal amount, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            return Error.Validation(
                code: "Payment.AmountInvalid",
                description: "Amount must be greater than zero.");
        }

        if (!Enum.TryParse<PaymentStatus>("Pending", true, out var pendingStatus))
        {
            return Error.Failure(
                code: "Payment.PendingStatusMissing",
                description: "Pending status is not defined in PaymentStatus enum.");
        }

        var payment = new Payment
        {
            TaskId = taskId,
            Amount = amount,
            Status = pendingStatus
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> ConfirmAsync(string paymentIntentId, bool succeeded, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(paymentIntentId))
        {
            return Error.Validation(
                code: "Payment.IntentIdRequired",
                description: "Payment intent id is required.");
        }

        if (!int.TryParse(paymentIntentId, out var paymentId))
        {
            return Error.Validation(
                code: "Payment.IntentIdInvalid",
                description: "Payment intent id is invalid.");
        }

        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment is null)
        {
            return Error.NotFound(
                code: "Payment.NotFound",
                description: $"Payment with ID {paymentId} not found.");
        }

        var targetStatusName = succeeded ? "Succeeded" : "Failed";
        if (!Enum.TryParse<PaymentStatus>(targetStatusName, true, out var targetStatus))
        {
            return Error.Failure(
                code: "Payment.StatusMissing",
                description: $"{targetStatusName} status is not defined in PaymentStatus enum.");
        }

        payment.Status = targetStatus;
        _paymentRepository.Update(payment);

        return Result.Success;
    }

    public async Task<ErrorOr<List<Payment>>> GetHistoryAsync(CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetAllAsync(cancellationToken);

        if (payments is null)
        {
            return Error.Failure(
                code: "GettingPayments.Null",
                description: "Failed to get payment history and returned null.");
        }

        if (payments.Count == 0)
        {
            return Error.NotFound(
                code: "GettingPayments.Empty",
                description: "No payments found.");
        }

        return payments.ToList();
    }
}