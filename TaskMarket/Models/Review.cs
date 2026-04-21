#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMarket.Models;

public class Review
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(TaskItem))]
    public int TaskId { get; set; }
    public virtual TaskItem TaskItem { get; set; } = null!;

    [ForeignKey(nameof(Reviewer))]
    public int ReviewerId { get; set; }
    public virtual User Reviewer { get; set; } = null!;

    [ForeignKey(nameof(Target))]
    public int TargetId { get; set; }
    public virtual User Target { get; set; } = null!;

    public int Rating { get; set; } // 1-5

    [Required]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}