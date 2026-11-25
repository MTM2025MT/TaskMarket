#nullable enable
using TaskMarket.Models;

namespace TaskMarket.Dtos;

// Entity -> DTO mappings used for responses, so EF navigation graphs (and user secrets) never get serialized
public static class DtoMappings
{
    public static TaskItemDto ToDto(this TaskItem task) => new()
    {
        Id = task.Id,
        ClientId = task.ClientId,
        CategoryId = task.CategoryId,
        ContractorId = task.ContractorId,
        Title = task.Title,
        Description = task.Description,
        Address = task.Address,
        Location = task.Location,
        Status = task.Status,
        CreatedAt = task.CreatedAt
    };

    public static TaskMediaDto ToDto(this TaskMedia media) => new()
    {
        Id = media.Id,
        TaskId = media.TaskId,
        MediaUrl = media.MediaUrl,
        MediaType = media.MediaType,
        CreatedAt = media.CreatedAt
    };

    public static JobOfferDto ToDto(this JobOffer offer) => new()
    {
        Id = offer.Id,
        TaskId = offer.TaskId,
        ClientId = offer.ClientId,
        Price = offer.Price,
        MinPrice = offer.MinPrice,
        MaxPrice = offer.MaxPrice,
        Location = offer.Location,
        Deadline = offer.Deadline,
        Status = offer.Status,
        CreatedAt = offer.CreatedAt
    };

    public static OfferApplicationDto ToDto(this OfferApplication application) => new()
    {
        Id = application.Id,
        OfferId = application.OfferId,
        ContractorId = application.ContractorId,
        Message = application.Message,
        PriceBid = application.PriceBid,
        Status = application.Status,
        CreatedAt = application.CreatedAt
    };

    public static HiringRequestDto ToDto(this HiringRequest request) => new()
    {
        Id = request.Id,
        TaskId = request.TaskId,
        ClientId = request.ClientId,
        ContractorId = request.ContractorId,
        Message = request.Message,
        PriceOffered = request.PriceOffered,
        Status = request.Status,
        SentAt = request.SentAt,
        RespondedAt = request.RespondedAt
    };

    public static MessageDto ToDto(this Message message) => new()
    {
        Id = message.Id,
        TaskId = message.TaskId,
        SenderId = message.SenderId,
        ReceiverId = message.ReceiverId,
        Content = message.Content,
        IsRead = message.IsRead,
        SentAt = message.SentAt
    };

    public static ReviewDto ToDto(this Review review) => new()
    {
        Id = review.Id,
        TaskId = review.TaskId,
        ReviewerId = review.ReviewerId,
        TargetId = review.TargetId,
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAt = review.CreatedAt
    };

    public static PaymentDto ToDto(this Payment payment) => new()
    {
        Id = payment.Id,
        TaskId = payment.TaskId,
        PayerId = payment.PayerId,
        PayeeId = payment.PayeeId,
        Amount = payment.Amount,
        Status = payment.Status,
        CreatedAt = payment.CreatedAt
    };

    public static ContractorDto ToDto(this Contractor contractor) => new()
    {
        Id = contractor.Id,
        BusinessName = contractor.BusinessName,
        Bio = contractor.Bio,
        HourlyRate = contractor.HourlyRate,
        RatingAvg = contractor.RatingAvg,
        IsVerified = contractor.IsVerified
    };
}