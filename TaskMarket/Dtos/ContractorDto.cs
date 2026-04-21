#nullable enable
namespace TaskMarket.Dtos;

public class ContractorDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal RatingAvg { get; set; }
    public bool IsVerified { get; set; }
}