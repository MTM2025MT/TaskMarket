#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMarket.Models;

public class Contractor: User
{


    [Required]
    public string BusinessName { get; set; } = string.Empty;

    public string? Bio { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal HourlyRate { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal RatingAvg { get; set; }

    public bool IsVerified { get; set; }
    public List<ServiceCategory> ContractorServiceCategories { get; set; } = new();
    public List<string>? CertificateFilesPaths { get; set; }
}