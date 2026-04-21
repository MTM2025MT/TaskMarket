#nullable enable
namespace TaskMarket.Models;

public enum TaskStatus
{
    Open,
    InProgress,
    Completed,
    Cancelled
}

public enum HiringRequestStatus
{
    Pending,
    Accepted,
    Declined
}

public enum OfferStatus
{
    Open,
    Closed
}

public enum ApplicationStatus
{
    Pending,
    Approved,
    Rejected
}

public enum PaymentStatus
{
    Pending,
    Released,
    Refunded
}