#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMarket.Models;

public class JobOffer
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(TaskItem))]
    public int TaskId { get; set; }
    public virtual TaskItem TaskItem { get; set; } = null!;

    [ForeignKey(nameof(Client))]
    public int ClientId { get; set; }
    public virtual Customer Client { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    public DateTime Deadline { get; set; }
    public OfferStatus Status { get; set; } = OfferStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}