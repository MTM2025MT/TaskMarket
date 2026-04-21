using Microsoft.EntityFrameworkCore;
using System.Numerics;
using TaskMarket.Models;
    
namespace TaskMarket;

public class EntitiesContext : DbContext
{
    public EntitiesContext(DbContextOptions<EntitiesContext> options)
        : base(options)
    {

    }

    // DB Sets (Models)
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Contractor> Contractors => Set<Contractor>();

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();

    public DbSet<HiringRequest> HiringRequests => Set<HiringRequest>();
    public DbSet<JobOffer> JobOffers => Set<JobOffer>();
    public DbSet<OfferApplication> OfferApplications => Set<OfferApplication>();

    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();

    // These were visible in your open tabs; included as well
    public DbSet<ContractorSkill> ContractorSkills => Set<ContractorSkill>();
    public DbSet<TaskMedia> TaskMedias => Set<TaskMedia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Match the relationship fixes you already put in ApplicationDbContext
        modelBuilder.Entity<User>().UseTpcMappingStrategy();
        modelBuilder.Entity<Customer>().ToTable("Customers");
        modelBuilder.Entity<Contractor>().ToTable("Contractor");
        // Messages: prevent multiple cascade paths (Sender/Receiver are both Users)
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        // Payments: prevent multiple cascade paths (Payer/Payee are both Users)
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

        // Reviews: prevent multiple cascade paths (Reviewer/Target are both Users)
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

        // HiringRequest: optional safety (Contractor side restricted)
        modelBuilder.Entity<HiringRequest>()
            .HasOne(h => h.Contractor)
            .WithMany()
            .HasForeignKey(h => h.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Decimal precision (avoid EF warnings for money)
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,2)");
        }
    }
}
