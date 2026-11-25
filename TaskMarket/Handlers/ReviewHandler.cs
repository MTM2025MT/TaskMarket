using ErrorOr;
using TaskMarket.Dtos;
using TaskMarket.IRepositories;
using TaskMarket.Models;

namespace TaskMarket.Handlers;

public sealed class ReviewHandler
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly IContractorRepository _contractorRepository;

    public ReviewHandler(
        IReviewRepository reviewRepository,
        ITaskItemRepository taskItemRepository,
        IContractorRepository contractorRepository)
    {
        _reviewRepository = reviewRepository;
        _taskItemRepository = taskItemRepository;
        _contractorRepository = contractorRepository;
    }

    // From TasksController: POST /api/tasks/{taskId}/reviews
    public async Task<ErrorOr<ReviewDto>> CreateTaskReviewAsync(int taskId, int reviewerId, CreateReviewRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            return Error.Validation(
                code: "Review.RatingInvalid",
                description: "Rating must be between 1 and 5.");
        }

        var task = await _taskItemRepository.GetByIdAsync(taskId, cancellationToken);
        if (task is null)
        {
            return Error.NotFound(
                code: "TaskItem.NotFound",
                description: $"Task with ID {taskId} not found.");
        }

        if (task.Status != TaskMarket.Models.TaskStatus.Completed || task.ContractorId is null)
        {
            return Error.Validation(
                code: "Review.TaskNotCompleted",
                description: "Only completed tasks can be reviewed.");
        }

        if (reviewerId != task.ClientId && reviewerId != task.ContractorId)
        {
            return Error.Forbidden(
                code: "Review.NotParticipant",
                description: "Only the task's customer or contractor can review it.");
        }

        if (await _reviewRepository.ExistsAsync(taskId, reviewerId, cancellationToken))
        {
            return Error.Conflict(
                code: "Review.Duplicate",
                description: "You have already reviewed this task.");
        }

        // Each party reviews the other one
        var targetId = reviewerId == task.ClientId ? task.ContractorId.Value : task.ClientId;

        var review = new Review
        {
            TaskId = taskId,
            ReviewerId = reviewerId,
            TargetId = targetId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        await _reviewRepository.AddAsync(review, cancellationToken);

        // Keep the contractor's average rating in sync (it drives GetBestContractors)
        if (targetId == task.ContractorId)
        {
            var contractor = await _contractorRepository.GetByIdAsync(targetId, cancellationToken);
            if (contractor is not null)
            {
                var ratings = await _reviewRepository.GetRatingsForTargetAsync(targetId, cancellationToken);
                ratings.Add(request.Rating);
                contractor.RatingAvg = Math.Round((decimal)ratings.Average(), 2);
            }
        }

        await _reviewRepository.SaveChangesAsync(cancellationToken);
        return review.ToDto();
    }

    // From ReviewsController: GET /api/contractors/{contractorId}/reviews
    public async Task<ErrorOr<List<Review>>> GetContractorReviewsAsync(int contractorId, CancellationToken cancellationToken = default)
    {
        return await _reviewRepository.GetByTargetIdAsync(contractorId, cancellationToken);
    }
}