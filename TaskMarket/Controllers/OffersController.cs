using Microsoft.AspNetCore.Mvc;
using TaskMarket.Handlers;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Controllers;

[ApiController]
[Route("api/offers")]
public class OffersController : ControllerBase
{
    private readonly IJobOfferRepository _jobOfferRepository;
    private readonly IOfferApplicationRepository _offerApplicationRepository;
    private readonly JobOfferHandler _jobOfferHandler;
    private readonly OfferApplicationHandler _offerApplicationHandler;

    public OffersController(
        IJobOfferRepository jobOfferRepository,
        IOfferApplicationRepository offerApplicationRepository,
        JobOfferHandler jobOfferHandler,
        OfferApplicationHandler offerApplicationHandler)
    {
        _jobOfferRepository = jobOfferRepository;
        _offerApplicationRepository = offerApplicationRepository;
        _jobOfferHandler = jobOfferHandler;
        _offerApplicationHandler = offerApplicationHandler;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOffer([FromBody] JobOffer request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultBox = await _jobOfferHandler.CreateOfferAsync(request);

        return resultBox.Match<IActionResult>(
            success => CreatedAtAction(nameof(GetOffers), new { id = request.Id }, success),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    [HttpGet]
    public async Task<IActionResult> GetOffers()
    {
        var resultBox = await _jobOfferHandler.GetOffersAsync();

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    [HttpPost("{offerId:int}/applications")]
    public async Task<IActionResult> CreateApplication(int offerId, [FromBody] OfferApplication request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var resultBox = await _offerApplicationHandler.CreateApplicationAsync(offerId, request);

        return resultBox.Match<IActionResult>(
            success => CreatedAtAction(nameof(GetApplications), new { offerId }, success),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    [HttpGet("{offerId:int}/applications")]
    public async Task<IActionResult> GetApplications(int offerId)
    {
        var resultBox = await _offerApplicationHandler.GetApplicationsAsync(offerId);

        return resultBox.Match<IActionResult>(
            success => Ok(success),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }

    [HttpPost("{offerId:int}/applications/{applicationId:int}/accept")]
    public async Task<IActionResult> AcceptApplication(int offerId, int applicationId)
    {
        var resultBox = await _offerApplicationHandler.AcceptApplicationAsync(offerId, applicationId);

        return resultBox.Match<IActionResult>(
            success => NoContent(),
            failure => StatusCode(StatusCodes.Status500InternalServerError, failure)
        );
    }
}