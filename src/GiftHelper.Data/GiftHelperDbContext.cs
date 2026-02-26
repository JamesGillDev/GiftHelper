using GiftHelper.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GiftHelper.Data;

public class GiftHelperDbContext(DbContextOptions<GiftHelperDbContext> options) : DbContext(options)
{
    public DbSet<Recipient> Recipients => Set<Recipient>();

    public DbSet<Occasion> Occasions => Set<Occasion>();

    public DbSet<GiftIdea> GiftIdeas => Set<GiftIdea>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Recipient>(entity =>
        {
            entity.Property(x => x.Name).IsRequired().HasMaxLength(150);
            entity.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<Occasion>(entity =>
        {
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(x => x.Date);
            entity
                .HasOne(x => x.Recipient)
                .WithMany(x => x.Occasions)
                .HasForeignKey(x => x.RecipientId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<GiftIdea>(entity =>
        {
            entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
            entity.Property(x => x.PriceEstimate).HasPrecision(18, 2);
            entity.Property(x => x.PricePaid).HasPrecision(18, 2);
            entity.Property(x => x.EstimatedMinPrice).HasPrecision(18, 2);
            entity.Property(x => x.EstimatedMaxPrice).HasPrecision(18, 2);
            entity.Property(x => x.Tags).HasMaxLength(500);
            entity.Property(x => x.SeedId).HasMaxLength(120);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.SeedId);
            entity
                .HasOne(x => x.Recipient)
                .WithMany(x => x.GiftIdeas)
                .HasForeignKey(x => x.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(x => x.Occasion)
                .WithMany(x => x.GiftIdeas)
                .HasForeignKey(x => x.OccasionId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    public override int SaveChanges()
    {
        ApplyTimestamps();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyTimestamps()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            switch (entry.Entity)
            {
                case Recipient recipient:
                    if (entry.State == EntityState.Added)
                    {
                        recipient.CreatedUtc = utcNow;
                    }

                    recipient.UpdatedUtc = utcNow;
                    break;

                case Occasion occasion:
                    if (entry.State == EntityState.Added)
                    {
                        occasion.CreatedUtc = utcNow;
                    }

                    occasion.UpdatedUtc = utcNow;
                    break;

                case GiftIdea giftIdea:
                    if (entry.State == EntityState.Added)
                    {
                        giftIdea.CreatedUtc = utcNow;
                    }

                    giftIdea.UpdatedUtc = utcNow;
                    break;
            }
        }
    }
}
