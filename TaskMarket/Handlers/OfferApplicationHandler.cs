using ErrorOr;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class OfferApplicationHandler
{
    private readonly IOfferApplicationRepository _offerApplicationRepository;

    public OfferApplicationHandler(IOfferApplicationRepository offerApplicationRepository)
    {
        _offerApplicationRepository = offerApplicationRepository;
    }

    // From OffersController: POST /api/offers/{offerId}/applications
    public async Task<ErrorOr<Success>> CreateApplicationAsync(int offerId, OfferApplication request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Error.Validation(
                code: "OfferApplication.Null",
                description: "Offer application cannot be null.");
        }

        request.OfferId = offerId;

        await _offerApplicationRepository.AddAsync(request, cancellationToken);
        return Result.Success;
    }

    // From OffersController: GET /api/offers/{offerId}/applications
    public async Task<ErrorOr<List<OfferApplication>>> GetApplicationsAsync(int offerId, CancellationToken cancellationToken = default)
    {
        var applications = await _offerApplicationRepository.GetAllAsync(cancellationToken);

        if (applications is null)
        {
            return Error.Failure(
                code: "GettingOfferApplications.Null",
                description: "Failed to get offer applications and returned null.");
        }

        var offerApplications = applications.Where(a => a.OfferId == offerId).ToList();

        if (offerApplications.Count == 0)
        {
            return Error.NotFound(
                code: "GettingOfferApplications.Empty",
                description: $"No applications found for offer with ID {offerId}.");
        }

        return offerApplications;
    }

    // From OffersController: POST /api/offers/{offerId}/applications/{applicationId}/accept
    public async Task<ErrorOr<Success>> AcceptApplicationAsync(int offerId, int applicationId, CancellationToken cancellationToken = default)
    {
        var application = await _offerApplicationRepository.GetByIdAsync(applicationId, cancellationToken);

        if (application is null)
        {
            return Error.NotFound(
                code: "OfferApplication.NotFound",
                description: $"Application with ID {applicationId} was not found.");
        }

        if (application.OfferId != offerId)
        {
            return Error.Validation(
                code: "OfferApplication.OfferMismatch",
                description: "Application does not belong to the provided offer.");
        }

        if (!Enum.TryParse<ApplicationStatus>("Accepted", true, out var acceptedStatus))
        {
            return Error.Failure(
                code: "OfferApplication.AcceptedStatusMissing",
                description: "Accepted status is not defined in ApplicationStatus enum.");
        }

        application.Status = acceptedStatus;
        _offerApplicationRepository.Update(application);

        return Result.Success;
    }
}