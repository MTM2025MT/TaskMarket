using Microsoft.AspNetCore.Mvc;
using TaskMarket.Handlers;
using TaskMarket.IRepositories;

namespace TaskMarket.Controllers;

[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _messageRepository;
    private readonly MessageHandler _messageHandler;
    private readonly ITaskItemRepository _TaskItemRepository;

    public MessagesController(IMessageRepository messageRepository, MessageHandler messageHandler, ITaskItemRepository taskItemRepository)
    {
        _messageRepository = messageRepository;
        _messageHandler = messageHandler;
        _TaskItemRepository = taskItemRepository;
    }

    [HttpPut("{messageId:int}/read")]
    public async Task<IActionResult> MarkAsRead(int messageId)
    {
        var resultBox = await _messageHandler.MarkAsReadAsync(messageId);

        return resultBox.Match<IActionResult>(
            success => NoContent(),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    [HttpGet("GetAllMesseges/{userid:int}")]
    public async Task<IActionResult> GetAllMessgesAsync(int userid)
    {
       var messeges=await _messageRepository.GetUserMesseges(userid);
        if (messeges!=null)
        {
            return Ok(messeges);
        }
        throw new  Exception("messges failed to run");
    }
}