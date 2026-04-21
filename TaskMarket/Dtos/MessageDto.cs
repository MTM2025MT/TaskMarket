#nullable enable
using System;

namespace TaskMarket.Dtos;

public class MessageDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
}