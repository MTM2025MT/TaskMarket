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
}