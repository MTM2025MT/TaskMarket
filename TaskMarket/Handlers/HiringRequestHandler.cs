using ErrorOr;
using TaskMarket.Dtos;
using TaskMarket.IRepositories;
using TaskMarket.Models;
namespace TaskMarket.Handlers;

public sealed class HiringRequestHandler
{
    private readonly IHiringRequestRepository _hiringRequestRepository;
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly IContractorRepository _contractorRepository;

    public HiringRequestHandler(
        IHiringRequestRepository hiringRequestRepository,
        ITaskItemRepository taskItemRepository,
        IContractorRepository contractorRepository)
    {
        _hiringRequestRepository = hiringRequestRepository;
        _taskItemRepository = taskItemRepository;
        _contractorRepository = contractorRepository;
    }

    // From HiringRequestsController: POST /api/hiring-requests
    public async Task<ErrorOr<HiringRequestDto>> CreateAsync(int clientId, CreateHiringRequestRequestDto request, CancellationToken cancellationToken = default)
    {
        var task = await _taskItemRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
        {
            return Error.NotFound(
                code: "TaskItem.NotFound",
                description: $"Task with ID {request.TaskId} not found.");
        }

        if (task.ClientId != clientId)
        {
            return Error.Forbidden(
                code: "HiringRequest.NotTaskOwner",
                description: "You can only send hiring requests for your own tasks.");
        }

        if (task.ContractorId is not null)
        {
            return Error.Conflict(
                code: "HiringRequest.TaskAlreadyAssigned",
                description: "This task already has a contractor.");
        }

        if (await _contractorRepository.GetByIdAsync(request.ContractorId, cancellationToken) is null)
        {
            return Error.NotFound(
                code: "Contractor.NotFound",
                description: $"Contractor with ID {request.ContractorId} not found.");
        }

        var hiringRequest = new HiringRequest
        {
            TaskId = task.Id,
            ClientId = clientId,
            ContractorId = request.ContractorId,
            Message = request.Message,
            PriceOffered = request.PriceOffered
        };

        var rowsAffected = await _hiringRequestRepository.AddAsync(hiringRequest, cancellationToken);
        if (rowsAffected == 0)
        {
            return Error.Failure(
                code: "HiringRequest.CreateFailed",
                description: "Failed to create the hiring request.");
        }
        return hiringRequest.ToDto();
    }

    // From HiringRequestsController: GET /api/hiring-requests
    // Returns only the requests the caller sent (customer) or received (contractor)
    public async Task<ErrorOr<List<HiringRequest>>> GetAllAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _hiringRequestRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    // From HiringRequestsController: PUT /api/hiring-requests/{requestId}/status
    public async Task<ErrorOr<Success>> UpdateStatusAsync(int requestId, int userId, string status, CancellationToken cancellationToken = default)
    {
        var hiringRequest = await _hiringRequestRepository.GetByIdAsync(requestId, cancellationToken);
        if (hiringRequest is null)
        {
            return Error.NotFound(
                code: "HiringRequest.NotFound",
                description: "Hiring request not found.");
        }

        if (hiringRequest.ContractorId != userId)
        {
            return Error.Forbidden(
                code: "HiringRequest.NotRecipient",
                description: "Only the contractor who received this request can respond to it.");
        }

        //check if the provided status is valid
        if (!Enum.TryParse(status, true, out HiringRequestStatus newStatus)
            || newStatus is not (HiringRequestStatus.Accepted or HiringRequestStatus.Declined))
        {
            return Error.Validation(
                code: "HiringRequest.InvalidStatus",
                description: "Status must be 'Accepted' or 'Declined'.");
        }

        if (hiringRequest.Status != HiringRequestStatus.Pending)
        {
            return Error.Conflict(
                code: "HiringRequest.AlreadyAnswered",
                description: "This hiring request was already answered.");
        }

        if (newStatus == HiringRequestStatus.Accepted)
        {
            var task = await _taskItemRepository.GetByIdAsync(hiringRequest.TaskId, cancellationToken);
            if (task is null)
            {
                return Error.NotFound(
                    code: "TaskItem.NotFound",
                    description: $"Task with ID {hiringRequest.TaskId} not found.");
            }

            if (task.ContractorId is not null && task.ContractorId != userId)
            {
                return Error.Conflict(
                    code: "HiringRequest.TaskAlreadyAssigned",
                    description: "This task already has another contractor.");
            }

            task.ContractorId = userId;
            task.Status = TaskMarket.Models.TaskStatus.InProgress;
        }

        hiringRequest.Status = newStatus;
        hiringRequest.RespondedAt = DateTime.UtcNow;

        //finally, update the hiring request (and the task, if accepted) in the database
        var rowsAffected = await _hiringRequestRepository.Update(hiringRequest, cancellationToken);
        if (rowsAffected == 0)
        {
            return Error.Failure(
                code: "HiringRequest.UpdateFailed",
                description: "Failed to update the hiring request status.");
        }
        return Result.Success;
    }
}