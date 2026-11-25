#nullable enable
using System.ComponentModel.DataAnnotations;

namespace TaskMarket.Dtos;

public class CreateTaskRequestDto
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? Address { get; set; }

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }
}