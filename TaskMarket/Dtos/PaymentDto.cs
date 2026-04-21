#nullable enable
using System;
using TaskMarket.Models;

namespace TaskMarket.Dtos;

public class PaymentDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int PayerId { get; set; }
    public int PayeeId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}