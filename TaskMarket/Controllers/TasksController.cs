using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMarket.Dtos;
using TaskMarket.Handlers;
using TaskMarket.Helpers;

namespace TaskMarket.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ProblemHandler
{
    private readonly TaskItemHandler _taskItemHandler;
    private readonly TaskMediaHandler _taskMediaHandler;
    private readonly MessageHandler _messageHandler;
    private readonly ReviewHandler _reviewHandler;

    public TasksController(
        TaskItemHandler taskItemHandler,
        TaskMediaHandler taskMediaHandler,
        MessageHandler messageHandler,
        ReviewHandler reviewHandler)
    {
        _taskItemHandler = taskItemHandler;
        _taskMediaHandler = taskMediaHandler;
        _messageHandler = messageHandler;
        _reviewHandler = reviewHandler;
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequestDto request)
    {
        var resultBox = await _taskItemHandler.CreateTaskAsync(User.GetUserId(), request);

        return resultBox.Match<IActionResult>(
            created => CreatedAtAction(nameof(GetTaskById), new { taskId = created.Id }, created),
            failure => Problem(failure)
        );
    }

    [HttpPost("{taskId:int}/media")]
    [Authorize]
    public async Task<IActionResult> AddTaskMedia(int taskId, [FromBody] AddTaskMediaRequestDto request)
    {
        var resultBox = await _taskMediaHandler.AddTaskMediaAsync(taskId, User.GetUserId(), request);

        return resultBox.Match<IActionResult>(
            created => StatusCode(StatusCodes.Status201Created, created),
            failure => Problem(failure)
        );
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        var resultBox = await _taskItemHandler.GetTasksAsync();

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => Problem(failure)
        );
    }

    [HttpGet("{taskId:int}")]
    public async Task<IActionResult> GetTaskById(int taskId)
    {
        var resultBox = await _taskItemHandler.GetTaskByIdAsync(taskId);

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => Problem(failure)
        );
    }

    [HttpGet("GetTasksByCustomerId/{customerId:int}")]
    public async Task<IActionResult> GetTasksByCustomerId(int customerId)
    {
        var resultBox = await _taskItemHandler.GetTasksByCustomerIdAsync(customerId);

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => Problem(failure)
        );
    }

    [HttpPut("{taskId:int}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateTaskStatus(int taskId, [FromBody] UpdateTaskStatusRequest request)
    {
        var resultBox = await _taskItemHandler.UpdateTaskStatusAsync(taskId, User.GetUserId(), request.Status);

        return resultBox.Match<IActionResult>(
            success => NoContent(),
            failure => Problem(failure)
        );
    }

    [HttpGet("{taskId:int}/messages")]
    [Authorize]
    public async Task<IActionResult> GetTaskMessages(int taskId)
    {
        var resultBox = await _messageHandler.GetTaskMessagesAsync(taskId, User.GetUserId());

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => Problem(failure)
        );
    }

    [HttpPost("{taskId:int}/messages")]
    [Authorize]
    public async Task<IActionResult> SendTaskMessage(int taskId, [FromBody] SendMessageRequestDto request)
    {
        var resultBox = await _messageHandler.SendTaskMessageAsync(taskId, User.GetUserId(), request.Content);

        return resultBox.Match<IActionResult>(
            created => StatusCode(StatusCodes.Status201Created, created),
            failure => Problem(failure)
        );
    }

    [HttpPost("{taskId:int}/reviews")]
    [Authorize]
    public async Task<IActionResult> CreateTaskReview(int taskId, [FromBody] CreateReviewRequestDto request)
    {
        var resultBox = await _reviewHandler.CreateTaskReviewAsync(taskId, User.GetUserId(), request);

        return resultBox.Match<IActionResult>(
            created => StatusCode(StatusCodes.Status201Created, created),
            failure => Problem(failure)
        );
    }

    public sealed record UpdateTaskStatusRequest(string Status);
}