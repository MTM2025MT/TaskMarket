#nullable enable
using System;
using TaskMarket.Models;

namespace TaskMarket.Dtos;

public class HiringRequestDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int ClientId { get; set; }
    public int ContractorId { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal PriceOffered { get; set; }
    public HiringRequestStatus Status { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}