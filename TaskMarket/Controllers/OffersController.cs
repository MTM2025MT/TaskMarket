
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMarket.Dtos;
using TaskMarket.Handlers;
using TaskMarket.Helpers;

namespace TaskMarket.Controllers;

[ApiController]
[Route("api/offers")]
public class OffersController : ProblemHandler
{
    private readonly JobOfferHandler _jobOfferHandler;
    private readonly OfferApplicationHandler _offerApplicationHandler;
    public OffersController(
        JobOfferHandler jobOfferHandler,
        OfferApplicationHandler offerApplicationHandler)
    {
        _jobOfferHandler = jobOfferHandler;
        _offerApplicationHandler = offerApplicationHandler;
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateOffer([FromBody] CreateJobOfferRequestDto request)
    {
        var resultBox = await _jobOfferHandler.CreateOfferAsync(User.GetUserId(), request);

        return resultBox.Match<IActionResult>(
            created => StatusCode(StatusCodes.Status201Created, created),
            failure => Problem(failure)
        );
    }

    [HttpGet]
    public async Task<IActionResult> GetOffers()
    {
        var resultBox = await _jobOfferHandler.GetOffersAsync();
        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => Problem(failure)
        );
    }

    [HttpGet("GetOffersByCustomerId/{customerId:int}")]
    public async Task<IActionResult> GetOffersByCustomerId(int customerId)
    {
        var resultBox = await _jobOfferHandler.GetOffersByCustomerIdAsync(customerId);

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => Problem(failure)
        );
    }

    [HttpPost("{offerId:int}/applications")]
    [Authorize(Roles = "Contractor")]
    public async Task<IActionResult> CreateApplication(int offerId, [FromBody] CreateOfferApplicationRequestDto request)
    {
        var resultBox = await _offerApplicationHandler.CreateApplicationAsync(offerId, User.GetUserId(), request);

        return resultBox.Match<IActionResult>(
            created => CreatedAtAction(nameof(GetApplications), new { offerId }, created),
            failure => Problem(failure)
        );
    }

    // The offer owner sees all applications; a contractor only sees their own
    [HttpGet("{offerId:int}/applications")]
    [Authorize]
    public async Task<IActionResult> GetApplications(int offerId)
    {
        var resultBox = await _offerApplicationHandler.GetApplicationsAsync(offerId, User.GetUserId());

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => Problem(failure)
        );
    }

    [HttpPost("{offerId:int}/applications/{applicationId:int}/accept")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> AcceptApplication(int offerId, int applicationId)
    {
        var resultBox = await _offerApplicationHandler.AcceptApplicationAsync(offerId, applicationId, User.GetUserId());

        return resultBox.Match<IActionResult>(
            success => NoContent(),
            failure => Problem(failure)
        );
    }
}