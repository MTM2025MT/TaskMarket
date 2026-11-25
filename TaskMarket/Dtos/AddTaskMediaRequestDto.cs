#nullable enable
using System.ComponentModel.DataAnnotations;

namespace TaskMarket.Dtos;

public class AddTaskMediaRequestDto
{
    [Required]
    public string MediaUrl { get; set; } = string.Empty;

    // e.g., "Image" or "Video"
    [Required]
    public string MediaType { get; set; } = string.Empty;
}