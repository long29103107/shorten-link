using Microsoft.EntityFrameworkCore;

namespace ShortenLink.Infrastructure.Persistence;

public sealed class ShortLinkDbContext : DbContext
{
    public ShortLinkDbContext(DbContextOptions<ShortLinkDbContext> options)
        : base(options)
    {
    }

    public DbSet<ShortLinkRecord> ShortLinks => Set<ShortLinkRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<ShortLinkRecord>(entity =>
        {
            entity.ToTable("short_links");
            entity.HasKey(link => link.Code);

            entity.Property(link => link.Code)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(link => link.OriginalUrl)
                .HasMaxLength(4096)
                .IsRequired();

            entity.Property(link => link.CreatedAt)
                .IsRequired();

            entity.Property(link => link.IsActive)
                .IsRequired();

            entity.HasIndex(link => link.Code)
                .IsUnique();

            entity.HasIndex(link => link.CreatedAt);
            entity.HasIndex(link => link.ExpiresAt);
            entity.HasIndex(link => link.IsActive);
        });
    }
}
