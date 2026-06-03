using ErrorOr;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class JobOfferHandler
{
    private readonly IJobOfferRepository _jobOfferRepository;

    public JobOfferHandler(IJobOfferRepository jobOfferRepository)
    {
        _jobOfferRepository = jobOfferRepository;
    }

    // From OffersController: POST /api/offers
    public async Task<ErrorOr<Success>> CreateOfferAsync(JobOffer request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Error.Validation(
                code: "JobOffer.Null",
                description: "Job offer cannot be null.");
        }

        await _jobOfferRepository.AddAsync(request, cancellationToken);
        
        return Result.Success;
    }

    // From OffersController: GET /api/offers
    public async Task<ErrorOr<List<JobOffer>>> GetOffersAsync(CancellationToken cancellationToken = default)
    {
        var offers = await _jobOfferRepository.GetAllAsync(cancellationToken);

        if (offers is null)
        {
            return Error.Failure(
                code: "GettingAllJobOffers.Null",
                description: "Failed to get all job offers and returned null.");
        }
        
        if (offers.Count == 0)
        {
            return Error.NotFound(
                code: "GettingAllJobOffers.Empty",
                description: "No job offers found.");
        }

        return offers;
    }

    // From OffersController: GET /api/offers/GetOffersByCustomerId/{customerId}
    public async Task<ErrorOr<List<JobOffer>>> GetOffersByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        if (customerId <= 0)
        {
            return Error.Validation(
                code: "JobOffer.InvalidCustomerId",
                description: "Customer ID must be greater than zero.");
        }

        var offers = await _jobOfferRepository.GetOffersByCustomerIdAsync(customerId, cancellationToken);

        if (offers is null)
        {
            return Error.Failure(
                code: "GettingJobOffersByCustomerId.Null",
                description: "Failed to get job offers and returned null.");
        }

        if (offers.Count == 0)
        {
            return Error.NotFound(
                code: "GettingJobOffersByCustomerId.Empty",
                description: $"No job offers found for customer ID {customerId}.");
        }

        return offers;
    }
}