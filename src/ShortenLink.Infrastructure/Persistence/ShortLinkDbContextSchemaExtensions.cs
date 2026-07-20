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

    public static async Task EnsureSecurityIdentitySchemaAsync(
        this ShortLinkDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        if (dbContext.Database.IsSqlite())
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "shorten_link_security_custom_roles" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_shorten_link_security_custom_roles" PRIMARY KEY,
                    "Name" TEXT NOT NULL,
                    "Permissions" TEXT NOT NULL,
                    "IsEnabled" INTEGER NOT NULL,
                    "CreatedAt" TEXT NOT NULL
                );
                """,
                cancellationToken).ConfigureAwait(false);

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "shorten_link_security_users" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_shorten_link_security_users" PRIMARY KEY,
                    "Username" TEXT NOT NULL,
                    "DisplayName" TEXT NOT NULL,
                    "PasswordHash" TEXT NOT NULL,
                    "RoleIds" TEXT NOT NULL,
                    "IsEnabled" INTEGER NOT NULL,
                    "IsHidden" INTEGER NOT NULL,
                    "IsBootstrap" INTEGER NOT NULL,
                    "CreatedAt" TEXT NOT NULL
                );
                """,
                cancellationToken).ConfigureAwait(false);

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "shorten_link_security_user_api_keys" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_shorten_link_security_user_api_keys" PRIMARY KEY,
                    "UserId" TEXT NOT NULL,
                    "DisplayName" TEXT NOT NULL,
                    "KeyHash" TEXT NOT NULL,
                    "IsEnabled" INTEGER NOT NULL,
                    "CreatedAt" TEXT NOT NULL
                );
                """,
                cancellationToken).ConfigureAwait(false);

            await EnsureSqliteSecurityIdentityIndexesAsync(dbContext, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (dbContext.Database.IsNpgsql())
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "shorten_link_security_custom_roles" (
                    "Id" character varying(128) NOT NULL CONSTRAINT "PK_shorten_link_security_custom_roles" PRIMARY KEY,
                    "Name" character varying(256) NOT NULL,
                    "Permissions" text NOT NULL,
                    "IsEnabled" boolean NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL
                );
                """,
                cancellationToken).ConfigureAwait(false);

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "shorten_link_security_users" (
                    "Id" character varying(128) NOT NULL CONSTRAINT "PK_shorten_link_security_users" PRIMARY KEY,
                    "Username" character varying(256) NOT NULL,
                    "DisplayName" character varying(256) NOT NULL,
                    "PasswordHash" character varying(1024) NOT NULL,
                    "RoleIds" text NOT NULL,
                    "IsEnabled" boolean NOT NULL,
                    "IsHidden" boolean NOT NULL,
                    "IsBootstrap" boolean NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL
                );
                """,
                cancellationToken).ConfigureAwait(false);

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "shorten_link_security_user_api_keys" (
                    "Id" character varying(128) NOT NULL CONSTRAINT "PK_shorten_link_security_user_api_keys" PRIMARY KEY,
                    "UserId" character varying(128) NOT NULL,
                    "DisplayName" character varying(256) NOT NULL,
                    "KeyHash" character varying(128) NOT NULL,
                    "IsEnabled" boolean NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL
                );
                """,
                cancellationToken).ConfigureAwait(false);

            await EnsurePostgresSecurityIdentityIndexesAsync(dbContext, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task EnsureSqliteSecurityIdentityIndexesAsync(
        ShortLinkDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_shorten_link_security_custom_roles_Name"
            ON "shorten_link_security_custom_roles" ("Name");
            """,
            cancellationToken).ConfigureAwait(false);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_shorten_link_security_custom_roles_IsEnabled"
            ON "shorten_link_security_custom_roles" ("IsEnabled");
            """,
            cancellationToken).ConfigureAwait(false);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_shorten_link_security_custom_roles_CreatedAt"
            ON "shorten_link_security_custom_roles" ("CreatedAt");
            """,
            cancellationToken).ConfigureAwait(false);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_shorten_link_security_users_Username"
            ON "shorten_link_security_users" ("Username");
            """,
            cancellationToken).ConfigureAwait(false);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_shorten_link_security_users_IsEnabled"
            ON "shorten_link_security_users" ("IsEnabled");
            """,
            cancellationToken).ConfigureAwait(false);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_shorten_link_security_users_IsHidden"
            ON "shorten_link_security_users" ("IsHidden");
            """,
            cancellationToken).ConfigureAwait(false);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_shorten_link_security_user_api_keys_UserId"
            ON "shorten_link_security_user_api_keys" ("UserId");
            """,
            cancellationToken).ConfigureAwait(false);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_shorten_link_security_user_api_keys_KeyHash"
            ON "shorten_link_security_user_api_keys" ("KeyHash");
            """,
            cancellationToken).ConfigureAwait(false);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "IX_shorten_link_security_user_api_keys_IsEnabled"
            ON "shorten_link_security_user_api_keys" ("IsEnabled");
            """,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsurePostgresSecurityIdentityIndexesAsync(
        ShortLinkDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_shorten_link_security_custom_roles_Name"
            ON "shorten_link_security_custom_roles" ("Name");
            CREATE INDEX IF NOT EXISTS "IX_shorten_link_security_custom_roles_IsEnabled"
            ON "shorten_link_security_custom_roles" ("IsEnabled");
            CREATE INDEX IF NOT EXISTS "IX_shorten_link_security_custom_roles_CreatedAt"
            ON "shorten_link_security_custom_roles" ("CreatedAt");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_shorten_link_security_users_Username"
            ON "shorten_link_security_users" ("Username");
            CREATE INDEX IF NOT EXISTS "IX_shorten_link_security_users_IsEnabled"
            ON "shorten_link_security_users" ("IsEnabled");
            CREATE INDEX IF NOT EXISTS "IX_shorten_link_security_users_IsHidden"
            ON "shorten_link_security_users" ("IsHidden");
            CREATE INDEX IF NOT EXISTS "IX_shorten_link_security_user_api_keys_UserId"
            ON "shorten_link_security_user_api_keys" ("UserId");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_shorten_link_security_user_api_keys_KeyHash"
            ON "shorten_link_security_user_api_keys" ("KeyHash");
            CREATE INDEX IF NOT EXISTS "IX_shorten_link_security_user_api_keys_IsEnabled"
            ON "shorten_link_security_user_api_keys" ("IsEnabled");
            """,
            cancellationToken).ConfigureAwait(false);
    }
}
