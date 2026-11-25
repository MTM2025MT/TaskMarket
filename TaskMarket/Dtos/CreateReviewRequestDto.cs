#nullable enable
using System.ComponentModel.DataAnnotations;

namespace TaskMarket.Dtos;

public class CreateReviewRequestDto
{
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    public string Comment { get; set; } = string.Empty;
}