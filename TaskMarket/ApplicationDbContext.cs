using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;
using System.Net;
using TaskMarket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema; // Needed for [ForeignKey] and [Column]

public class ApplicationDbContext : IdentityDbContext<User, IdentityRoleApplication, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DB Sets
    public DbSet<User> Users { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Contractor> Contractors { get; set; }
    //public DbSet<Address> Addresses { get; set; }
    public DbSet<ContractorSkill> ContractorSkills { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<ServiceCategory> ServiceCategories { get; set; }
    public DbSet<TaskMedia> TaskMedias { get; set; }
    
    public DbSet<HiringRequest> HiringRequests { get; set; }
    public DbSet<JobOffer> JobOffers { get; set; }
    public DbSet<OfferApplication> OfferApplications { get; set; }

    public DbSet<Message> Messages { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ====================================================
        // 1. FIX: MESSAGES (Prevent Multiple Cascade Paths)
        // ====================================================
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict); // Do NOT auto-delete if Sender is deleted

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict); // Do NOT auto-delete if Receiver is deleted

        modelBuilder.Entity<Message>()
            .HasOne(m => m.TaskItem)
            .WithMany(t => t.Messages)
            .HasForeignKey(m => m.TaskId)
            .OnDelete(DeleteBehavior.Restrict); // Do NOT auto-delete if TaskItem is deleted

        // ====================================================
        // 2. FIX: PAYMENTS (Prevent Multiple Cascade Paths)
        // ====================================================
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Payer)
            .WithMany()
            .HasForeignKey(p => p.PayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Payee)
            .WithMany()
            .HasForeignKey(p => p.PayeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.TaskItem)
            .WithMany()
            .HasForeignKey(p => p.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        // ====================================================
        // 3. FIX: REVIEWS (Prevent Multiple Cascade Paths)
        // ====================================================
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Target)
            .WithMany()
            .HasForeignKey(r => r.TargetId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.TaskItem)
            .WithMany()
            .HasForeignKey(r => r.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        // ====================================================
        // 4. FIX: HIRING REQUESTS (Prevent Multiple Cascade Paths)
        // ====================================================
        modelBuilder.Entity<HiringRequest>()
            .HasOne(h => h.Contractor)
            .WithMany()
            .HasForeignKey(h => h.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HiringRequest>()
            .HasOne(h => h.TaskItem)
            .WithMany()
            .HasForeignKey(h => h.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HiringRequest>()
            .HasOne(h => h.Client)
            .WithMany() 
            .HasForeignKey(h => h.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // ====================================================
        // 5. FIX: JOB OFFERS + APPLICATIONS
        // ====================================================
        modelBuilder.Entity<JobOffer>()
            .HasOne(j => j.TaskItem)
            .WithMany()
            .HasForeignKey(j => j.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<JobOffer>()
            .HasOne(j => j.Client)
            .WithMany()
            .HasForeignKey(j => j.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OfferApplication>()
            .HasOne(a => a.JobOffer)
            .WithMany()
            .HasForeignKey(a => a.OfferId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OfferApplication>()
            .HasOne(a => a.Contractor)
            .WithMany()
            .HasForeignKey(a => a.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ====================================================
        // 6. FIX: TASK MEDIA
        // ====================================================
        modelBuilder.Entity<TaskMedia>()
            .HasOne(tm => tm.TaskItem)
            .WithMany()
            .HasForeignKey(tm => tm.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        // ====================================================
        // 7. FIX: TASK ITEM LINKS
        // ====================================================
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Client)
            .WithMany()
            .HasForeignKey(t => t.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Contractor)
            .WithMany()
            .HasForeignKey(t => t.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ====================================================
        // 8. FIX: CONTRACTOR SKILLS
        // ====================================================
        modelBuilder.Entity<ContractorSkill>()
            .HasKey(cs => new { cs.ContractorId, cs.CategoryId });

        modelBuilder.Entity<ContractorSkill>()
            .HasOne(cs => cs.Contractor)
            .WithMany()
            .HasForeignKey(cs => cs.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ContractorSkill>()
            .HasOne(cs => cs.ServiceCategory)
            .WithMany()
            .HasForeignKey(cs => cs.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ====================================================
        // 9. DECIMAL PRECISION (To avoid Warnings)
        // ====================================================
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Contractor>().ToTable("Contractors");
        modelBuilder.Entity<Customer>().ToTable("Customers");

        // EF Core often warns if you don't specify precision for money
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,2)");
        }
    }
}