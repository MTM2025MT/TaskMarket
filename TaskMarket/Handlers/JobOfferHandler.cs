using ErrorOr;
using TaskMarket.Dtos;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class JobOfferHandler
{
    private readonly IJobOfferRepository _jobOfferRepository;
    private readonly ITaskItemRepository _taskItemRepository;

    public JobOfferHandler(IJobOfferRepository jobOfferRepository, ITaskItemRepository taskItemRepository)
    {
        _jobOfferRepository = jobOfferRepository;
        _taskItemRepository = taskItemRepository;
    }

    // From OffersController: POST /api/offers
    public async Task<ErrorOr<JobOfferDto>> CreateOfferAsync(int clientId, CreateJobOfferRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.MinPrice > request.MaxPrice)
        {
            return Error.Validation(
                code: "JobOffer.InvalidPriceRange",
                description: "MinPrice cannot be greater than MaxPrice.");
        }

        if (request.Deadline <= DateTime.UtcNow)
        {
            return Error.Validation(
                code: "JobOffer.DeadlineInPast",
                description: "Deadline must be in the future.");
        }

        if (request.Latitude.HasValue != request.Longitude.HasValue)
        {
            return Error.Validation(
                code: "JobOffer.LocationIncomplete",
                description: "Latitude and longitude must be provided together.");
        }

        var task = await _taskItemRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
        {
            return Error.NotFound(
                code: "TaskItem.NotFound",
                description: $"Task with ID {request.TaskId} not found.");
        }

        if (task.ClientId != clientId)
        {
            return Error.Forbidden(
                code: "JobOffer.NotTaskOwner",
                description: "You can only create offers for your own tasks.");
        }

        var offer = new JobOffer
        {
            TaskId = task.Id,
            ClientId = clientId,
            Price = request.Price,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            Deadline = request.Deadline,
            Location = request.Latitude.HasValue
                ? new GeoCoordinate(request.Latitude.Value, request.Longitude!.Value)
                : task.Location
        };

        await _jobOfferRepository.AddAsync(offer, cancellationToken);
        await _jobOfferRepository.SaveChangesAsync(cancellationToken);

        return offer.ToDto();
    }

    // From OffersController: GET /api/offers
    public async Task<ErrorOr<List<JobOffer>>> GetOffersAsync(CancellationToken cancellationToken = default)
    {
        return await _jobOfferRepository.GetAllAsync(cancellationToken);
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

        return await _jobOfferRepository.GetOffersByCustomerIdAsync(customerId, cancellationToken);
    }
}