using Microsoft.EntityFrameworkCore;

namespace ShortenLink.Infrastructure.Persistence;

public sealed class ShortLinkDbContext : DbContext
{
    public ShortLinkDbContext(DbContextOptions<ShortLinkDbContext> options)
        : base(options)
    {
    }

    public DbSet<ShortLinkRecord> ShortLinks => Set<ShortLinkRecord>();

    public DbSet<ShortLinkClickRecord> ShortLinkClicks => Set<ShortLinkClickRecord>();

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

        modelBuilder.Entity<ShortLinkClickRecord>(entity =>
        {
            entity.ToTable("short_link_clicks");
            entity.HasKey(click => click.Id);

            entity.Property(click => click.Id)
                .ValueGeneratedOnAdd();

            entity.Property(click => click.ShortCode)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(click => click.ClickedAtUtc)
                .IsRequired();

            entity.Property(click => click.RemoteIpAddress)
                .HasMaxLength(256);

            entity.Property(click => click.UserAgent)
                .HasMaxLength(1024);

            entity.Property(click => click.Referrer)
                .HasMaxLength(2048);

            entity.HasIndex(click => click.ShortCode);
            entity.HasIndex(click => click.ClickedAtUtc);
            entity.HasIndex(click => new { click.ShortCode, click.ClickedAtUtc });
        });
    }
}
