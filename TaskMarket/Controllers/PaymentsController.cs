using Microsoft.AspNetCore.Mvc;
using TaskMarket.Handlers;
using TaskMarket.IRepositories;

namespace TaskMarket.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly PaymentsHandler _paymentsHandler;

    public PaymentsController(IPaymentRepository paymentRepository, PaymentsHandler paymentsHandler)
    {
        _paymentRepository = paymentRepository;
        _paymentsHandler = paymentsHandler;
    }

    [HttpPost("intent")]
    public async Task<IActionResult> CreateIntent([FromBody] CreatePaymentIntentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultBox = await _paymentsHandler.CreateIntentAsync(request.TaskId, request.Amount);

        return resultBox.Match<IActionResult>(
            success => NoContent(),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmPaymentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultBox = await _paymentsHandler.ConfirmAsync(request.PaymentIntentId, request.Succeeded);

        return resultBox.Match<IActionResult>(
            success => NoContent(),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory()
    {
        var resultBox = await _paymentsHandler.GetHistoryAsync();

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    public sealed record CreatePaymentIntentRequest(int TaskId, decimal Amount);
    public sealed record ConfirmPaymentRequest(string PaymentIntentId, bool Succeeded);
}