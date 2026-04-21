#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMarket.Models;

public class Customer:User
{
    [Key]
    public int Id { get; set; }



    public string? StripeCustomerId { get; set; }

    [Required]
    public string BillingAddress { get; set; } = string.Empty;
}