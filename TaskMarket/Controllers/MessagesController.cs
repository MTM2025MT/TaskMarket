using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMarket.Handlers;
using TaskMarket.Helpers;

namespace TaskMarket.Controllers;

[ApiController]
[Authorize]
[Route("api/messages")]
public class MessagesController : ProblemHandler
{
    private readonly MessageHandler _messageHandler;

    public MessagesController(MessageHandler messageHandler)
    {
        _messageHandler = messageHandler;
    }

    [HttpPut("{messageId:int}/read")]
    public async Task<IActionResult> MarkAsRead(int messageId)
    {
        var resultBox = await _messageHandler.MarkAsReadAsync(messageId, User.GetUserId());

        return resultBox.Match<IActionResult>(
            success => NoContent(),
            failure => Problem(failure)
        );
    }

    [HttpGet("GetAllMesseges/{userid:int}")]
    public async Task<IActionResult> GetAllMessgesAsync(int userid)
    {
        var resultBox = await _messageHandler.GetUserMessagesAsync(userid, User.GetUserId());

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => Problem(failure)
        );
    }
}