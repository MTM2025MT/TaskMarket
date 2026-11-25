using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMarket.Handlers;
using TaskMarket.Helpers;

namespace TaskMarket.Controllers;

[ApiController]
[Authorize]
[Route("api/payments")]
public class PaymentsController : ProblemHandler
{
    private readonly PaymentsHandler _paymentsHandler;

    public PaymentsController(PaymentsHandler paymentsHandler)
    {
        _paymentsHandler = paymentsHandler;
    }

    [HttpPost("intent")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateIntent([FromBody] CreatePaymentIntentRequest request)
    {
        var resultBox = await _paymentsHandler.CreateIntentAsync(request.TaskId, User.GetUserId(), request.Amount);

        return resultBox.Match<IActionResult>(
            created => StatusCode(StatusCodes.Status201Created, created),
            failure => Problem(failure)
        );
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmPaymentRequest request)
    {
        var resultBox = await _paymentsHandler.ConfirmAsync(request.PaymentIntentId, request.Succeeded, User.GetUserId());

        return resultBox.Match<IActionResult>(
            success => NoContent(),
            failure => Problem(failure)
        );
    }

    // Payments the caller made or received
    [HttpGet]
    public async Task<IActionResult> GetHistory()
    {
        var resultBox = await _paymentsHandler.GetHistoryAsync(User.GetUserId());

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => Problem(failure)
        );
    }

    public sealed record CreatePaymentIntentRequest(int TaskId, decimal Amount);
    public sealed record ConfirmPaymentRequest(string PaymentIntentId, bool Succeeded);
}