#nullable enable
using System;
using TaskMarket.Models;

namespace TaskMarket.Dtos;

public class TaskItemDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int CategoryId { get; set; }
    public int? ContractorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public TaskMarket.Models.TaskStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}