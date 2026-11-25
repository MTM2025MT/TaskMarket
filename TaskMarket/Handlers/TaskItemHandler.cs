using ErrorOr;
using TaskMarket.Dtos;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class TaskItemHandler
{
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly IServiceCategoryRepository _serviceCategoryRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IContractorRepository _contractorRepository;

    public TaskItemHandler(
        ITaskItemRepository taskItemRepository,
        IServiceCategoryRepository serviceCategoryRepository,
        ICustomerRepository customerRepository,
        IContractorRepository contractorRepository)
    {
        _taskItemRepository = taskItemRepository;
        _serviceCategoryRepository = serviceCategoryRepository;
        _customerRepository = customerRepository;
        _contractorRepository = contractorRepository;
    }

    // From TasksController: POST /api/tasks
    public async Task<ErrorOr<TaskItemDto>> CreateTaskAsync(int clientId, CreateTaskRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Error.Validation(
                code: "TaskItem.TitleRequired",
                description: "Task title is required.");
        }

        if (await _customerRepository.GetByIdAsync(clientId, cancellationToken) is null)
        {
            return Error.Forbidden(
                code: "TaskItem.NotACustomer",
                description: "Only customer accounts can create tasks.");
        }

        if (await _serviceCategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken) is null)
        {
            return Error.Validation(
                code: "TaskItem.CategoryNotFound",
                description: $"Category with ID {request.CategoryId} does not exist.");
        }

        if (request.Latitude.HasValue != request.Longitude.HasValue)
        {
            return Error.Validation(
                code: "TaskItem.LocationIncomplete",
                description: "Latitude and longitude must be provided together.");
        }

        var task = new TaskItem
        {
            ClientId = clientId,
            CategoryId = request.CategoryId,
            Title = request.Title.Trim(),
            Description = request.Description,
            Address = request.Address,
            Location = request.Latitude.HasValue
                ? new GeoCoordinate(request.Latitude.Value, request.Longitude!.Value)
                : null
        };

        await _taskItemRepository.AddAsync(task, cancellationToken);
        await _taskItemRepository.SaveChangesAsync(cancellationToken);
        return task.ToDto();
    }

    // From TasksController: GET /api/tasks
    public async Task<ErrorOr<List<TaskItem>>> GetTasksAsync(CancellationToken cancellationToken = default)
    {
        return await _taskItemRepository.GetAllAsync(cancellationToken);
    }

    // From TasksController: GET /api/tasks/{taskId}
    public async Task<ErrorOr<TaskItem>> GetTaskByIdAsync(int taskId, CancellationToken cancellationToken = default)
    {
        var task = await _taskItemRepository.GetByIdAsync(taskId, cancellationToken);

        if (task is null)
        {
            return Error.NotFound(
                code: "TaskItem.NotFound",
                description: $"Task with ID {taskId} not found.");
        }

        return task;
    }

    // From TasksController: PUT /api/tasks/{taskId}/status
    public async Task<ErrorOr<Success>> UpdateTaskStatusAsync(int taskId, int userId, string status, CancellationToken cancellationToken = default)
    {
        var task = await _taskItemRepository.GetByIdAsync(taskId, cancellationToken);

        if (task is null)
        {
            return Error.NotFound(
                code: "TaskItem.NotFound",
                description: $"Task with ID {taskId} not found.");
        }

        if (task.ClientId != userId && task.ContractorId != userId)
        {
            return Error.Forbidden(
                code: "TaskItem.NotParticipant",
                description: "Only the task's customer or assigned contractor can change its status.");
        }

        if (!Enum.TryParse(status, true, out TaskMarket.Models.TaskStatus newStatus) || !Enum.IsDefined(newStatus))
        {
            return Error.Validation(
                code: "TaskItem.InvalidStatus",
                description: "Invalid task status value.");
        }

        if (newStatus == TaskMarket.Models.TaskStatus.Completed && task.ContractorId is null)
        {
            return Error.Validation(
                code: "TaskItem.NoContractor",
                description: "A task cannot be completed before a contractor is assigned.");
        }

        // Count each completion once, when the task first moves into Completed
        if (newStatus == TaskMarket.Models.TaskStatus.Completed && task.Status != TaskMarket.Models.TaskStatus.Completed)
        {
            var contractor = await _contractorRepository.GetByIdAsync(task.ContractorId!.Value, cancellationToken);
            if (contractor is not null)
            {
                contractor.CompletedTaskCount = (contractor.CompletedTaskCount ?? 0) + 1;
            }
        }

        task.Status = newStatus;
        await _taskItemRepository.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }

    // From TasksController: GET /api/tasks/GetTasksByCustomerId/{customerId}
    public async Task<ErrorOr<List<TaskItem>>> GetTasksByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        if (customerId <= 0)
        {
            return Error.Validation(
                code: "TaskItem.InvalidCustomerId",
                description: "Customer ID must be greater than zero.");
        }

        return await _taskItemRepository.GetTasksByCustomerIdAsync(customerId, cancellationToken);
    }
}