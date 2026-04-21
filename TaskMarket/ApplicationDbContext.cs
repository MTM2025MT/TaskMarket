using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;
using System.Net;
using TaskMarket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema; // Needed for [ForeignKey] and [Column]
public class ApplicationDbContext : DbContext
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

    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<ServiceCategory> ServiceCategories { get; set; }

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

        // ====================================================
        // 4. FIX: HIRING REQUESTS (Optional Safety)
        // ====================================================
        // Since HiringRequest links to Client AND Contractor (who both link to User),
        // restricting one side is safer.
        modelBuilder.Entity<HiringRequest>()
            .HasOne(h => h.Contractor)
            .WithMany()
            .HasForeignKey(h => h.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ====================================================
        // 5. DECIMAL PRECISION (To avoid Warnings)
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