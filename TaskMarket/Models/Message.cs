#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMarket.Models;

public class Message
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(TaskItem))]
    public int TaskId { get; set; }
    public virtual TaskItem TaskItem { get; set; } = null!;

    [ForeignKey(nameof(Sender))]
    public int SenderId { get; set; }
    public virtual User Sender { get; set; } = null!;

    [ForeignKey(nameof(Receiver))]
    public int ReceiverId { get; set; }
    public virtual User Receiver { get; set; } = null!;

    [Required]
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}