using ErrorOr;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class ReviewHandler
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    // From TasksController: POST /api/tasks/{taskId}/reviews
    public async Task<ErrorOr<Success>> CreateTaskReviewAsync(int taskId, Review request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Error.Validation(
                code: "Review.Null",
                description: "Review cannot be null.");
        }

        if (request.Rating < 1 || request.Rating > 5)
        {
            return Error.Validation(
                code: "Review.RatingInvalid",
                description: "Rating must be between 1 and 5.");
        }

        request.TaskId = taskId;

        await _reviewRepository.AddAsync(request, cancellationToken);
        return Result.Success;
    }

    // From ReviewsController: GET /api/contractors/{contractorId}/reviews
    public async Task<ErrorOr<List<Review>>> GetContractorReviewsAsync(int contractorId, CancellationToken cancellationToken = default)
    {
        var reviews = await _reviewRepository.GetAllAsync(cancellationToken);

        if (reviews is null)
        {
            return Error.Failure(
                code: "GettingContractorReviews.Null",
                description: "Failed to get contractor reviews and returned null.");
        }

        var contractorReviews = reviews.Where(r => r.TargetId == contractorId).ToList();

        if (contractorReviews.Count == 0)
        {
            return Error.NotFound(
                code: "GettingContractorReviews.Empty",
                description: $"No reviews found for contractor with ID {contractorId}.");
        }

        return contractorReviews;
    }
}