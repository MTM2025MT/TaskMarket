using Microsoft.AspNetCore.Mvc;
using TaskMarket.Handlers;
using TaskMarket.IRepositories;

namespace TaskMarket.Controllers;

[ApiController]
[Route("api")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ReviewHandler _reviewHandler;

    public ReviewsController(IReviewRepository reviewRepository, ReviewHandler reviewHandler)
    {
        _reviewRepository = reviewRepository;
        _reviewHandler = reviewHandler;
    }

    [HttpGet("contractors/{contractorId:int}/reviews")]
    public async Task<IActionResult> GetContractorReviews(int contractorId)
    {
        var resultBox = await _reviewHandler.GetContractorReviewsAsync(contractorId);

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }
}