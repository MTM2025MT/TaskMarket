#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMarket.Models;

public class HiringRequest
{
    [Key]
    public int Id { get; set; } 

    [ForeignKey(nameof(TaskItem))]
    public int TaskId { get; set; }
    public virtual TaskItem TaskItem { get; set; } = null!;

    [ForeignKey(nameof(Client))]
    public int ClientId { get; set; }
    public virtual Customer Client { get; set; } = null!;

    [ForeignKey(nameof(Contractor))]
    public int ContractorId { get; set; }
    public virtual Contractor Contractor { get; set; } = null!;

    [Required]
    public string Message { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public  decimal PriceOffered { get; set; }

    public HiringRequestStatus Status { get; set; } = HiringRequestStatus.Pending;
    
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
} 