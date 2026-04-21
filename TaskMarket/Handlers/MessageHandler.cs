using ErrorOr;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class MessageHandler
{
    private readonly IMessageRepository _messageRepository;

    public MessageHandler(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    // From TasksController: GET /api/tasks/{taskId}/messages
    public async Task<ErrorOr<List<Message>>> GetTaskMessagesAsync(int taskId, CancellationToken cancellationToken = default)
    {
        var messages = await _messageRepository.GetAllAsync(cancellationToken);

        if (messages is null)
        {
            return Error.Failure(
                code: "GettingTaskMessages.Null",
                description: "Failed to get messages and returned null.");
        }

        // Filter messages by TaskId since the repository interface only exposes GetAllAsync
        var taskMessages = messages.Where(m => m.TaskId == taskId).ToList();

        if (taskMessages.Count == 0)
        {
            return Error.NotFound(
                code: "TaskMessages.Empty",
                description: $"No messages found for task with ID {taskId}.");
        }

        return taskMessages;
    }

    // From TasksController: POST /api/tasks/{taskId}/messages
    public async Task<ErrorOr<Success>> SendTaskMessageAsync(int taskId, Message request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Error.Validation(
                code: "Message.Null",
                description: "Message request cannot be null.");
        }

        // Ensure the message is explicitly linked to the requested task
        request.TaskId = taskId;

        await _messageRepository.AddAsync(request, cancellationToken);
        
        return Result.Success;
    }

    // From MessagesController: PUT /api/messages/{messageId}/read
    public async Task<ErrorOr<Success>> MarkAsReadAsync(int messageId, CancellationToken cancellationToken = default)
    {
        var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken);

        if (message is null)
        {
            return Error.NotFound(
                code: "Message.NotFound",
                description: $"Message with ID {messageId} not found.");
        }

        // Mark as read and update via repository
        message.IsRead = true;
        _messageRepository.Update(message);

        return Result.Success;
    }
}