#nullable enable
using System;
using TaskMarket.Models;

namespace TaskMarket.Dtos;

public class OfferApplicationDto
{
    public int Id { get; set; }
    public int OfferId { get; set; }
    public int ContractorId { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal? PriceBid { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}