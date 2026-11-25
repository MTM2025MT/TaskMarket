using ErrorOr;
using TaskMarket.Dtos;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class PaymentsHandler
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITaskItemRepository _taskItemRepository;

    public PaymentsHandler(IPaymentRepository paymentRepository, ITaskItemRepository taskItemRepository)
    {
        _paymentRepository = paymentRepository;
        _taskItemRepository = taskItemRepository;
    }

    public async Task<ErrorOr<PaymentDto>> CreateIntentAsync(int taskId, int payerId, decimal amount, CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            return Error.Validation(
                code: "Payment.AmountInvalid",
                description: "Amount must be greater than zero.");
        }

        var task = await _taskItemRepository.GetByIdAsync(taskId, cancellationToken);
        if (task is null)
        {
            return Error.NotFound(
                code: "TaskItem.NotFound",
                description: $"Task with ID {taskId} not found.");
        }

        if (task.ClientId != payerId)
        {
            return Error.Forbidden(
                code: "Payment.NotTaskOwner",
                description: "Only the task's customer can pay for it.");
        }

        if (task.ContractorId is null)
        {
            return Error.Validation(
                code: "Payment.NoContractor",
                description: "The task has no assigned contractor to pay.");
        }

        var payment = new Payment
        {
            TaskId = taskId,
            PayerId = task.ClientId,
            PayeeId = task.ContractorId.Value,
            Amount = amount,
            Status = PaymentStatus.Pending
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);
        return payment.ToDto();
    }

    public async Task<ErrorOr<Success>> ConfirmAsync(string paymentIntentId, bool succeeded, int userId, CancellationToken cancellationToken = default)
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

        if (payment.PayerId != userId)
        {
            return Error.Forbidden(
                code: "Payment.NotPayer",
                description: "Only the payer can confirm this payment.");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return Error.Conflict(
                code: "Payment.NotPending",
                description: "Only pending payments can be confirmed.");
        }

        payment.Status = succeeded ? PaymentStatus.Released : PaymentStatus.Failed;
        await _paymentRepository.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }

    public async Task<ErrorOr<List<Payment>>> GetHistoryAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _paymentRepository.GetByUserIdAsync(userId, cancellationToken);
    }
}