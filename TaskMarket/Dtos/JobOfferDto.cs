#nullable enable
using System;
using TaskMarket.Models;

namespace TaskMarket.Dtos;

public class JobOfferDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int ClientId { get; set; }
    public decimal Price { get; set; }
    public DateTime Deadline { get; set; }
    public OfferStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}