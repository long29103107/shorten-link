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

    public DbSet<ShortenLinkSecurityAssignmentRecord> SecurityAssignments => Set<ShortenLinkSecurityAssignmentRecord>();

    public DbSet<ShortenLinkCustomRoleRecord> SecurityCustomRoles => Set<ShortenLinkCustomRoleRecord>();

    public DbSet<ShortenLinkSecurityUserRecord> SecurityUsers => Set<ShortenLinkSecurityUserRecord>();

    public DbSet<ShortenLinkUserApiKeyRecord> SecurityUserApiKeys => Set<ShortenLinkUserApiKeyRecord>();

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

        modelBuilder.Entity<ShortenLinkSecurityAssignmentRecord>(entity =>
        {
            entity.ToTable("shorten_link_security_assignments");
            entity.HasKey(assignment => assignment.CredentialKeyHash);

            entity.Property(assignment => assignment.CredentialKeyHash)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(assignment => assignment.Name)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(assignment => assignment.RolesJson)
                .HasColumnName("Roles")
                .IsRequired();

            entity.Property(assignment => assignment.PermissionsJson)
                .HasColumnName("Permissions")
                .IsRequired();

            entity.Property(assignment => assignment.IsEnabled)
                .IsRequired();

            entity.Property(assignment => assignment.CreatedAt)
                .IsRequired();

            entity.HasIndex(assignment => assignment.IsEnabled);
            entity.HasIndex(assignment => assignment.CreatedAt);
        });

        modelBuilder.Entity<ShortenLinkCustomRoleRecord>(entity =>
        {
            entity.ToTable("shorten_link_security_custom_roles");
            entity.HasKey(role => role.Id);

            entity.Property(role => role.Id)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(role => role.Name)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(role => role.PermissionsJson)
                .HasColumnName("Permissions")
                .IsRequired();

            entity.Property(role => role.IsEnabled)
                .IsRequired();

            entity.Property(role => role.CreatedAt)
                .IsRequired();

            entity.HasIndex(role => role.Name)
                .IsUnique();
            entity.HasIndex(role => role.IsEnabled);
            entity.HasIndex(role => role.CreatedAt);
        });

        modelBuilder.Entity<ShortenLinkSecurityUserRecord>(entity =>
        {
            entity.ToTable("shorten_link_security_users");
            entity.HasKey(user => user.Id);

            entity.Property(user => user.Id)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(user => user.Username)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(user => user.DisplayName)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(1024)
                .IsRequired();

            entity.Property(user => user.RoleIdsJson)
                .HasColumnName("RoleIds")
                .IsRequired();

            entity.Property(user => user.IsEnabled)
                .IsRequired();

            entity.Property(user => user.IsHidden)
                .IsRequired();

            entity.Property(user => user.IsBootstrap)
                .IsRequired();

            entity.Property(user => user.CreatedAt)
                .IsRequired();

            entity.HasIndex(user => user.Username)
                .IsUnique();
            entity.HasIndex(user => user.IsEnabled);
            entity.HasIndex(user => user.IsHidden);
        });

        modelBuilder.Entity<ShortenLinkUserApiKeyRecord>(entity =>
        {
            entity.ToTable("shorten_link_security_user_api_keys");
            entity.HasKey(apiKey => apiKey.Id);

            entity.Property(apiKey => apiKey.Id)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(apiKey => apiKey.UserId)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(apiKey => apiKey.DisplayName)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(apiKey => apiKey.KeyHash)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(apiKey => apiKey.IsEnabled)
                .IsRequired();

            entity.Property(apiKey => apiKey.CreatedAt)
                .IsRequired();

            entity.HasIndex(apiKey => apiKey.UserId);
            entity.HasIndex(apiKey => apiKey.KeyHash)
                .IsUnique();
            entity.HasIndex(apiKey => apiKey.IsEnabled);
        });
    }
}
