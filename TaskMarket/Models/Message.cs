#nullable enable
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMarket.Models;

[Index(nameof(TaskId))]
public class Message
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(TaskItem))]
 
    public required int TaskId { get; set; }
    public virtual TaskItem TaskItem { get; set; } = null!;

    [ForeignKey(nameof(Sender))]
    public required int SenderId { get; set; }
    public virtual User Sender { get; set; } = null!;

    [ForeignKey(nameof(Receiver))]
    public  int? ReceiverId { get; set; }
    public virtual User? Receiver { get; set; }

    [Required]
    public required string Content { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}