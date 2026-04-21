#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMarket.Models;

public class Payment
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(TaskItem))]
    public int TaskId { get; set; }
    public virtual TaskItem TaskItem { get; set; } = null!;

    [ForeignKey(nameof(Payer))]
    public int PayerId { get; set; }
    public virtual User Payer { get; set; } = null!;

    [ForeignKey(nameof(Payee))]
    public int PayeeId { get; set; }
    public virtual User Payee { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}