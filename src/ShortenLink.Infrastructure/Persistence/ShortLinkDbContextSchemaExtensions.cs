using Microsoft.EntityFrameworkCore;

namespace ShortenLink.Infrastructure.Persistence;

public static class ShortLinkDbContextSchemaExtensions
{
    public static async Task EnsureSecurityAssignmentsSchemaAsync(
        this ShortLinkDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        if (dbContext.Database.IsSqlite())
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "shorten_link_security_assignments" (
                    "CredentialKeyHash" TEXT NOT NULL CONSTRAINT "PK_shorten_link_security_assignments" PRIMARY KEY,
                    "Name" TEXT NOT NULL,
                    "Roles" TEXT NOT NULL,
                    "Permissions" TEXT NOT NULL,
                    "IsEnabled" INTEGER NOT NULL,
                    "CreatedAt" TEXT NOT NULL
                );
                """,
                cancellationToken).ConfigureAwait(false);

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE INDEX IF NOT EXISTS "IX_shorten_link_security_assignments_IsEnabled"
                ON "shorten_link_security_assignments" ("IsEnabled");
                """,
                cancellationToken).ConfigureAwait(false);

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE INDEX IF NOT EXISTS "IX_shorten_link_security_assignments_CreatedAt"
                ON "shorten_link_security_assignments" ("CreatedAt");
                """,
                cancellationToken).ConfigureAwait(false);

            return;
        }

        if (dbContext.Database.IsNpgsql())
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "shorten_link_security_assignments" (
                    "CredentialKeyHash" character varying(128) NOT NULL CONSTRAINT "PK_shorten_link_security_assignments" PRIMARY KEY,
                    "Name" character varying(256) NOT NULL,
                    "Roles" text NOT NULL,
                    "Permissions" text NOT NULL,
                    "IsEnabled" boolean NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL
                );
                """,
                cancellationToken).ConfigureAwait(false);

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE INDEX IF NOT EXISTS "IX_shorten_link_security_assignments_IsEnabled"
                ON "shorten_link_security_assignments" ("IsEnabled");
                """,
                cancellationToken).ConfigureAwait(false);

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE INDEX IF NOT EXISTS "IX_shorten_link_security_assignments_CreatedAt"
                ON "shorten_link_security_assignments" ("CreatedAt");
                """,
                cancellationToken).ConfigureAwait(false);
        }
    }
}
