#nullable enable
using System;

namespace TaskMarket.Dtos;

public class CustomerDto
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }

    public string? StripeCustomerId { get; set; }
    public string BillingAddress { get; set; } = string.Empty;
}