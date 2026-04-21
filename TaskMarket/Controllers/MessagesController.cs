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

    public MessagesController(IMessageRepository messageRepository, MessageHandler messageHandler)
    {
        _messageRepository = messageRepository;
        _messageHandler = messageHandler;
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
}