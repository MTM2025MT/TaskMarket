    #nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMarket.Models;

public class TaskItem
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Client")]
    public int ClientId { get; set; }
    public virtual Customer Client { get; set; } = null!;

    [ForeignKey("Category")]
    public int CategoryId { get; set; }
    public virtual ServiceCategory Category { get; set; } = null!;

    // Nullable: Contractor is only assigned later
    [ForeignKey("Contractor")]
    public int? ContractorId { get; set; }
    public virtual Contractor? Contractor { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation for related data
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}