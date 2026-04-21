using Microsoft.AspNetCore.Mvc;
using TaskMarket.Handlers;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly ITaskMediaRepository _taskMediaRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly TaskItemHandler _taskItemHandler;
    private readonly TaskMediaHandler _taskMediaHandler;
    private readonly MessageHandler _messageHandler;
    private readonly ReviewHandler _reviewHandler;

    public TasksController(
        ITaskItemRepository taskItemRepository,
        ITaskMediaRepository taskMediaRepository,
        IMessageRepository messageRepository,
        IReviewRepository reviewRepository,
        TaskItemHandler taskItemHandler,
        TaskMediaHandler taskMediaHandler,
        MessageHandler messageHandler,
        ReviewHandler reviewHandler)
    {
        _taskItemRepository = taskItemRepository;
        _taskMediaRepository = taskMediaRepository;
        _messageRepository = messageRepository;
        _reviewRepository = reviewRepository;
        _taskItemHandler = taskItemHandler;
        _taskMediaHandler = taskMediaHandler;
        _messageHandler = messageHandler;
        _reviewHandler = reviewHandler;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] TaskItem request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultBox = await _taskItemHandler.CreateTaskAsync(request);

        return resultBox.Match<IActionResult>(
            success => CreatedAtAction(nameof(GetTaskById), new { taskId = request.Id }, success),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    [HttpPost("{taskId:int}/media")]
    public async Task<IActionResult> AddTaskMedia(int taskId, [FromBody] TaskMedia request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultBox = await _taskMediaHandler.AddTaskMediaAsync(taskId, request);

        return resultBox.Match<IActionResult>(
            success => NoContent(),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        var resultBox = await _taskItemHandler.GetTasksAsync();

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    [HttpGet("{taskId:int}")]
    public async Task<IActionResult> GetTaskById(int taskId)
    {
        var resultBox = await _taskItemHandler.GetTaskByIdAsync(taskId);

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    [HttpPut("{taskId:int}/status")]
    public async Task<IActionResult> UpdateTaskStatus(int taskId, [FromBody] UpdateTaskStatusRequest request)
    {
        var resultBox = await _taskItemHandler.UpdateTaskStatusAsync(taskId, request.Status);

        return resultBox.Match<IActionResult>(
            success => NoContent(),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    [HttpGet("{taskId:int}/messages")]
    public async Task<IActionResult> GetTaskMessages(int taskId)
    {
        var resultBox = await _messageHandler.GetTaskMessagesAsync(taskId);

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    [HttpPost("{taskId:int}/messages")]
    public async Task<IActionResult> SendTaskMessage(int taskId, [FromBody] Message request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultBox = await _messageHandler.SendTaskMessageAsync(taskId, request);

        return resultBox.Match<IActionResult>(
            success => NoContent(),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    [HttpPost("{taskId:int}/reviews")]
    public async Task<IActionResult> CreateTaskReview(int taskId, [FromBody] Review request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultBox = await _reviewHandler.CreateTaskReviewAsync(taskId, request);

        return resultBox.Match<IActionResult>(
            success => NoContent(),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    public sealed record UpdateTaskStatusRequest(string Status);
}