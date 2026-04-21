  using Microsoft.AspNetCore.Mvc;
using TaskMarket.Handlers;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Controllers;

[ApiController]
[Route("api/hiring-requests")]
public class HiringRequestsController : ControllerBase
{
    private readonly IHiringRequestRepository _hiringRequestRepository;
    private readonly HiringRequestHandler _hiringRequestHandler;

    public HiringRequestsController(IHiringRequestRepository hiringRequestRepository, HiringRequestHandler hiringRequestHandler)
    {
        _hiringRequestRepository = hiringRequestRepository;
        _hiringRequestHandler = hiringRequestHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] HiringRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultBox = await _hiringRequestHandler.CreateAsync(request);

        return resultBox.Match<IActionResult>(

            success => CreatedAtAction(nameof(GetAll), new { id = request.Id, Contractor = request.Contractor.BusinessName, customer=request.Client.UserName }, success),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var resultBox = await _hiringRequestHandler.GetAllAsync();

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    [HttpPut("{requestId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int requestId, [FromBody] UpdateHiringRequestStatusRequest request)
    {
        var resultBox = await _hiringRequestHandler.UpdateStatusAsync(requestId, request.Status);

        return resultBox.Match<IActionResult>(
            success => NoContent(),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
            );
    }

    public sealed record UpdateHiringRequestStatusRequest(string Status);
}