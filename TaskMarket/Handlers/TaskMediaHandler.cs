using ErrorOr;
using TaskMarket.Dtos;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class TaskMediaHandler
{
    private readonly ITaskMediaRepository _taskMediaRepository;
    private readonly ITaskItemRepository _taskItemRepository;

    public TaskMediaHandler(ITaskMediaRepository taskMediaRepository, ITaskItemRepository taskItemRepository)
    {
        _taskMediaRepository = taskMediaRepository;
        _taskItemRepository = taskItemRepository;
    }

    // From TasksController: POST /api/tasks/{taskId}/media
    public async Task<ErrorOr<TaskMediaDto>> AddTaskMediaAsync(int taskId, int userId, AddTaskMediaRequestDto request, CancellationToken cancellationToken = default)
    {
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

        var task = await _taskItemRepository.GetByIdAsync(taskId, cancellationToken);
        if (task is null)
        {
            return Error.NotFound(
                code: "TaskItem.NotFound",
                description: $"Task with ID {taskId} not found.");
        }

        if (task.ClientId != userId)
        {
            return Error.Forbidden(
                code: "TaskMedia.NotOwner",
                description: "Only the task's customer can add media to it.");
        }

        var media = new TaskMedia
        {
            TaskId = taskId,
            MediaUrl = request.MediaUrl,
            MediaType = request.MediaType
        };

        await _taskMediaRepository.AddAsync(media, cancellationToken);
        await _taskMediaRepository.SaveChangesAsync(cancellationToken);
        return media.ToDto();
    }
}