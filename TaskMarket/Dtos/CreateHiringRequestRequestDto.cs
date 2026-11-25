#nullable enable
using System.ComponentModel.DataAnnotations;

namespace TaskMarket.Dtos;

public class CreateHiringRequestRequestDto
{
    [Range(1, int.MaxValue)]
    public int TaskId { get; set; }

    [Range(1, int.MaxValue)]
    public int ContractorId { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal PriceOffered { get; set; }
}