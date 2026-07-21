using ShortenLink.Core.Domain;
using ShortenLink.Core.Security;
using Xunit;

namespace ShortenLink.Core.Tests;

public sealed class BaseEntityTests
{
    [Fact]
    public void NewEntity_UsesUuidVersion7AndTracksAuditMetadata()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var updatedAt = createdAt.AddMinutes(1);
        var actorId = Guid.CreateVersion7();
        var entity = new ShortLinkEntity("docs001", new Uri("https://example.com/docs"), createdAt);

        Assert.Equal(7, entity.Id.Version);
        Assert.Equal(createdAt, entity.CreatedAt);
        Assert.Null(entity.CreatedBy);
        Assert.Null(entity.UpdatedBy);
        Assert.Null(entity.UpdatedAt);

        entity.MarkUpdated(actorId, updatedAt);

        Assert.Equal(actorId, entity.UpdatedBy);
        Assert.Equal(updatedAt, entity.UpdatedAt);
    }

    [Fact]
    public void SecurityEntities_KeepBusinessKeysSeparateFromTechnicalId()
    {
        var entity = new ShortenLinkSecurityUserEntity(
            "user-admin",
            "admin@example.com",
            "Admin",
            "password-hash",
            [],
            true,
            false,
            false,
            DateTimeOffset.UtcNow);

        Assert.Equal(7, entity.Id.Version);
        Assert.Equal("user-admin", entity.UserKey);
    }
}
