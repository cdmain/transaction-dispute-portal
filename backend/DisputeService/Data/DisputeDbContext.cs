using Microsoft.EntityFrameworkCore;
using DisputeService.Models;

namespace DisputeService.Data;

public class DisputeDbContext : DbContext
{
    public DisputeDbContext(DbContextOptions<DisputeDbContext> options) : base(options)
    {
    }

    public DbSet<Dispute> Disputes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Dispute>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DisputedAmount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.CustomerId).HasMaxLength(50);
            entity.Property(e => e.Reason).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ResolutionNotes).HasMaxLength(1000);
            entity.Property(e => e.TransactionReference).HasMaxLength(100);
            entity.Property(e => e.MerchantName).HasMaxLength(200);
            
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.TransactionId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
