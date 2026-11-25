using ErrorOr;
using TaskMarket.Dtos;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class MessageHandler
{
    private readonly IMessageRepository _messageRepository;
    private readonly ITaskItemRepository _taskItemRepository;

    public MessageHandler(IMessageRepository messageRepository, ITaskItemRepository taskItemRepository)
    {
        _messageRepository = messageRepository;
        _taskItemRepository = taskItemRepository;
    }

    // Task chat is between the task's customer and its assigned contractor
    public async Task<ErrorOr<TaskItem>> GetTaskForParticipantAsync(int taskId, int userId, CancellationToken cancellationToken = default)
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
                code: "TaskMessages.NotParticipant",
                description: "Only the task's customer and assigned contractor can use its chat.");
        }

        return task;
    }

    // From TasksController: GET /api/tasks/{taskId}/messages
    public async Task<ErrorOr<List<Message>>> GetTaskMessagesAsync(int taskId, int userId, CancellationToken cancellationToken = default)
    {
        var taskResult = await GetTaskForParticipantAsync(taskId, userId, cancellationToken);
        if (taskResult.IsError)
        {
            return taskResult.Errors;
        }

        return await _messageRepository.GetByTaskIdAsync(taskId, cancellationToken);
    }

    // From TasksController: POST /api/tasks/{taskId}/messages, and ChatHub.SendMessage
    public async Task<ErrorOr<MessageDto>> SendTaskMessageAsync(int taskId, int senderId, string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Error.Validation(
                code: "Message.ContentRequired",
                description: "Message content is required.");
        }

        var taskResult = await GetTaskForParticipantAsync(taskId, senderId, cancellationToken);
        if (taskResult.IsError)
        {
            return taskResult.Errors;
        }

        var task = taskResult.Value;

        // The receiver is the other party; it is null while the task has no contractor yet
        var message = new Message
        {
            TaskId = taskId,
            SenderId = senderId,
            ReceiverId = senderId == task.ClientId ? task.ContractorId : task.ClientId,
            Content = content
        };

        await _messageRepository.AddAsync(message, cancellationToken);
        await _messageRepository.SaveChangesAsync(cancellationToken);

        return message.ToDto();
    }

    // From MessagesController: PUT /api/messages/{messageId}/read
    public async Task<ErrorOr<Success>> MarkAsReadAsync(int messageId, int userId, CancellationToken cancellationToken = default)
    {
        var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken);

        if (message is null)
        {
            return Error.NotFound(
                code: "Message.NotFound",
                description: $"Message with ID {messageId} not found.");
        }

        if (message.ReceiverId != userId)
        {
            return Error.Forbidden(
                code: "Message.NotReceiver",
                description: "Only the receiver can mark a message as read.");
        }

        message.IsRead = true;
        await _messageRepository.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }

    // From MessagesController: GET /api/messages/GetAllMesseges/{userid}
    public async Task<ErrorOr<List<Message>>> GetUserMessagesAsync(int userId, int requesterId, CancellationToken cancellationToken = default)
    {
        if (userId != requesterId)
        {
            return Error.Forbidden(
                code: "Message.NotOwner",
                description: "You can only read your own messages.");
        }

        return await _messageRepository.GetUserMesseges(userId, cancellationToken);
    }
}