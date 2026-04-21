using ErrorOr;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class TaskItemHandler
{
    private readonly ITaskItemRepository _taskItemRepository;

    public TaskItemHandler(ITaskItemRepository taskItemRepository)
    {
        _taskItemRepository = taskItemRepository;
    }

    // From TasksController: POST /api/tasks
    public async Task<ErrorOr<Success>> CreateTaskAsync(TaskItem request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Error.Validation(
                code: "TaskItem.Null",
                description: "Task request cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Error.Validation(
                code: "TaskItem.TitleRequired",
                description: "Task title is required.");
        }

        await _taskItemRepository.AddAsync(request, cancellationToken);
        return Result.Success;
    }

    // From TasksController: GET /api/tasks
    public async Task<ErrorOr<List<TaskItem>>> GetTasksAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await _taskItemRepository.GetAllAsync(cancellationToken);

        if (tasks is null)
        {
            return Error.Failure(
                code: "GettingTasks.Null",
                description: "Failed to get tasks and returned null.");
        }

        if (tasks.Count == 0)
        {
            return Error.NotFound(
                code: "GettingTasks.Empty",
                description: "No tasks found.");
        }

        return tasks.ToList();
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
    public async Task<ErrorOr<Success>> UpdateTaskStatusAsync(int taskId, string status, CancellationToken cancellationToken = default)
    {
        var task = await _taskItemRepository.GetByIdAsync(taskId, cancellationToken);

        if (task is null)
        {
            return Error.NotFound(
                code: "TaskItem.NotFound",
                description: $"Task with ID {taskId} not found.");
        }

        if (!Enum.TryParse(status, true, out TaskMarket.Models.TaskStatus newStatus))
        {
            return Error.Validation(
                code: "TaskItem.InvalidStatus",
                description: "Invalid task status value.");
        }

        task.Status = newStatus;
        _taskItemRepository.Update(task);

        return Result.Success;
    }
}