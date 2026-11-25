using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMarket.Dtos;
using TaskMarket.Handlers;
using TaskMarket.Helpers;

namespace TaskMarket.Controllers;

[ApiController]
[Authorize]
[Route("api/hiring-requests")]
public class HiringRequestsController : ProblemHandler
{
    private readonly HiringRequestHandler _hiringRequestHandler;

    public HiringRequestsController(HiringRequestHandler hiringRequestHandler)
    {
        _hiringRequestHandler = hiringRequestHandler;
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Create([FromBody] CreateHiringRequestRequestDto request)
    {
        var resultBox = await _hiringRequestHandler.CreateAsync(User.GetUserId(), request);

        return resultBox.Match<IActionResult>(
            created => StatusCode(StatusCodes.Status201Created, created),
            failure => Problem(failure)
        );
    }

    // Requests the caller sent (customer) or received (contractor)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var resultBox = await _hiringRequestHandler.GetAllAsync(User.GetUserId());

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => Problem(failure)
        );
    }

    [HttpPut("{requestId:int}/status")]
    [Authorize(Roles = "Contractor")]
    public async Task<IActionResult> UpdateStatus(int requestId, [FromBody] UpdateHiringRequestStatusRequest request)
    {
        var resultBox = await _hiringRequestHandler.UpdateStatusAsync(requestId, User.GetUserId(), request.Status);

        return resultBox.Match<IActionResult>(
            success => NoContent(),
            failure => Problem(failure)
            );
    }

    public sealed record UpdateHiringRequestStatusRequest(string Status);
}