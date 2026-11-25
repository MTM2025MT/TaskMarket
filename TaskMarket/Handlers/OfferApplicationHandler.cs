using ErrorOr;
using TaskMarket.Dtos;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class OfferApplicationHandler
{
    private readonly IOfferApplicationRepository _offerApplicationRepository;
    private readonly IJobOfferRepository _jobOfferRepository;
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly IContractorRepository _contractorRepository;

    public OfferApplicationHandler(
        IOfferApplicationRepository offerApplicationRepository,
        IJobOfferRepository jobOfferRepository,
        ITaskItemRepository taskItemRepository,
        IContractorRepository contractorRepository)
    {
        _offerApplicationRepository = offerApplicationRepository;
        _jobOfferRepository = jobOfferRepository;
        _taskItemRepository = taskItemRepository;
        _contractorRepository = contractorRepository;
    }

    // From OffersController: POST /api/offers/{offerId}/applications
    public async Task<ErrorOr<OfferApplicationDto>> CreateApplicationAsync(int offerId, int contractorId, CreateOfferApplicationRequestDto request, CancellationToken cancellationToken = default)
    {
        var offer = await _jobOfferRepository.GetByIdAsync(offerId, cancellationToken);
        if (offer is null)
        {
            return Error.NotFound(
                code: "JobOffer.NotFound",
                description: $"Offer with ID {offerId} not found.");
        }

        if (offer.Status != OfferStatus.Open)
        {
            return Error.Conflict(
                code: "JobOffer.Closed",
                description: "This offer is no longer accepting applications.");
        }

        if (await _contractorRepository.GetByIdAsync(contractorId, cancellationToken) is null)
        {
            return Error.Forbidden(
                code: "OfferApplication.NotAContractor",
                description: "Only contractor accounts can apply to offers.");
        }

        if (await _offerApplicationRepository.ExistsAsync(offerId, contractorId, cancellationToken))
        {
            return Error.Conflict(
                code: "OfferApplication.Duplicate",
                description: "You have already applied to this offer.");
        }

        var application = new OfferApplication
        {
            OfferId = offerId,
            ContractorId = contractorId,
            Message = request.Message,
            PriceBid = request.PriceBid
        };

        await _offerApplicationRepository.AddAsync(application, cancellationToken);
        await _offerApplicationRepository.SaveChangesAsync(cancellationToken);
        return application.ToDto();
    }

    // From OffersController: GET /api/offers/{offerId}/applications
    // The offer owner sees every application; anyone else only sees their own
    public async Task<ErrorOr<List<OfferApplication>>> GetApplicationsAsync(int offerId, int userId, CancellationToken cancellationToken = default)
    {
        var offer = await _jobOfferRepository.GetByIdAsync(offerId, cancellationToken);
        if (offer is null)
        {
            return Error.NotFound(
                code: "JobOffer.NotFound",
                description: $"Offer with ID {offerId} not found.");
        }

        var applications = await _offerApplicationRepository.GetByOfferIdAsync(offerId, cancellationToken);

        return offer.ClientId == userId
            ? applications
            : applications.Where(a => a.ContractorId == userId).ToList();
    }

    // From OffersController: POST /api/offers/{offerId}/applications/{applicationId}/accept
    public async Task<ErrorOr<Success>> AcceptApplicationAsync(int offerId, int applicationId, int userId, CancellationToken cancellationToken = default)
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

        var offer = await _jobOfferRepository.GetByIdAsync(offerId, cancellationToken);
        if (offer is null)
        {
            return Error.NotFound(
                code: "JobOffer.NotFound",
                description: $"Offer with ID {offerId} not found.");
        }

        if (offer.ClientId != userId)
        {
            return Error.Forbidden(
                code: "JobOffer.NotOwner",
                description: "Only the offer owner can accept applications.");
        }

        if (offer.Status != OfferStatus.Open || application.Status != ApplicationStatus.Pending)
        {
            return Error.Conflict(
                code: "OfferApplication.NotPending",
                description: "This offer is closed or the application was already handled.");
        }

        var task = await _taskItemRepository.GetByIdAsync(offer.TaskId, cancellationToken);
        if (task is null)
        {
            return Error.NotFound(
                code: "TaskItem.NotFound",
                description: $"Task with ID {offer.TaskId} not found.");
        }

        // Approve this application, reject the other pending ones, close the offer and assign the task
        application.Status = ApplicationStatus.Approved;

        var otherApplications = await _offerApplicationRepository.GetByOfferIdAsync(offerId, cancellationToken);
        foreach (var other in otherApplications.Where(a => a.Id != application.Id && a.Status == ApplicationStatus.Pending))
        {
            other.Status = ApplicationStatus.Rejected;
        }

        offer.Status = OfferStatus.Closed;
        task.ContractorId = application.ContractorId;
        task.Status = TaskMarket.Models.TaskStatus.InProgress;

        // All repositories share the scoped DbContext, so this commits everything in one transaction
        await _offerApplicationRepository.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}