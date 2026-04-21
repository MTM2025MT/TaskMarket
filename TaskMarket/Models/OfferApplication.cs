#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMarket.Models;

public class OfferApplication
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(JobOffer))]
    public int OfferId { get; set; }
    public virtual JobOffer JobOffer { get; set; } = null!;

    [ForeignKey(nameof(Contractor))]
    public int ContractorId { get; set; }
    public virtual Contractor Contractor { get; set; } = null!;

    [Required]
    public string Message { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? PriceBid { get; set; } // Optional counter-offer

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}