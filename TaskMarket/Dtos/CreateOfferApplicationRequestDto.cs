#nullable enable
using System.ComponentModel.DataAnnotations;

namespace TaskMarket.Dtos;

public class CreateOfferApplicationRequestDto
{
    [Required]
    public string Message { get; set; } = string.Empty;

    // Optional counter-offer
    [Range(0, double.MaxValue)]
    public decimal? PriceBid { get; set; }
}