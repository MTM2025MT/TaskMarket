#nullable enable
using System;

namespace TaskMarket.Dtos;

public class TaskMediaDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}