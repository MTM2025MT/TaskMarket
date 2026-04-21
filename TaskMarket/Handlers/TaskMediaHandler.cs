using ErrorOr;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class TaskMediaHandler
{
    private readonly ITaskMediaRepository _taskMediaRepository;

    public TaskMediaHandler(ITaskMediaRepository taskMediaRepository)
    {
        _taskMediaRepository = taskMediaRepository;
    }

    // From TasksController: POST /api/tasks/{taskId}/media
    public async Task<ErrorOr<Success>> AddTaskMediaAsync(int taskId, TaskMedia request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Error.Validation(
                code: "TaskMedia.Null",
                description: "Task media cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(request.MediaUrl))
        {
            return Error.Validation(
                code: "TaskMedia.MediaUrlRequired",
                description: "Media URL is required.");
        }

        if (string.IsNullOrWhiteSpace(request.MediaType))
        {
            return Error.Validation(
                code: "TaskMedia.MediaTypeRequired",
                description: "Media type is required.");
        }

        request.TaskId = taskId;

        await _taskMediaRepository.AddAsync(request, cancellationToken);
        return Result.Success;
    }
}