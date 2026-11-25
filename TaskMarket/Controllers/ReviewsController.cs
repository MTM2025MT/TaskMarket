using Microsoft.AspNetCore.Mvc;
using TaskMarket.Handlers;

namespace TaskMarket.Controllers;

[ApiController]
[Route("api")]
public class ReviewsController : ProblemHandler
{
    private readonly ReviewHandler _reviewHandler;

    public ReviewsController(ReviewHandler reviewHandler)
    {
        _reviewHandler = reviewHandler;
    }

    [HttpGet("contractors/{contractorId:int}/reviews")]
    public async Task<IActionResult> GetContractorReviews(int contractorId)
    {
        var resultBox = await _reviewHandler.GetContractorReviewsAsync(contractorId);

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => Problem(failure)
        );
    }
}