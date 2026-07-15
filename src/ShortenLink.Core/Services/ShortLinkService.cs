using ShortenLink.Core.Domain;
using ShortenLink.Core.Generation;
using ShortenLink.Core.Repositories;

namespace ShortenLink.Core.Services;

public sealed class ShortLinkService : IShortLinkService
{
    private const int MaxCodeGenerationAttempts = 10;

    private readonly IShortLinkRepository repository;
    private readonly IShortLinkCache cache;
    private readonly IShortCodeGenerator codeGenerator;
    private readonly TimeProvider timeProvider;

    public ShortLinkService(
        IShortLinkRepository repository,
        IShortCodeGenerator codeGenerator,
        IShortLinkCache? cache = null,
        TimeProvider? timeProvider = null)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.codeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
        this.cache = cache ?? new DisabledShortLinkCache();
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public Task<IReadOnlyList<ShortLink>> ListRecentAsync(
        int limit = 100,
        DateTimeOffset? beforeCreatedAt = null,
        string? beforeCode = null,
        CancellationToken cancellationToken = default) =>
        repository.ListRecentAsync(Math.Clamp(limit, 1, 500), beforeCreatedAt, beforeCode, cancellationToken);

    public Task<IReadOnlyList<ShortLink>> ListRecentPageAsync(
        int skip,
        int limit = 100,
        CancellationToken cancellationToken = default) =>
        repository.ListRecentPageAsync(Math.Max(skip, 0), Math.Clamp(limit, 1, 500), cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        repository.CountAsync(cancellationToken);

    public async Task<CreateShortLinkResult> CreateAsync(
        CreateShortLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!ShortLinkUrlValidator.TryCreate(request.OriginalUrl, out var originalUrl))
        {
            return CreateShortLinkResult.Failure(
                ShortLinkErrorCodes.InvalidUrl,
                "Original URL must be an absolute HTTP or HTTPS URL.");
        }

        var now = timeProvider.GetUtcNow();
        if (request.ExpiresAt is null)
        {
            return CreateShortLinkResult.Failure(
                ShortLinkErrorCodes.InvalidExpiration,
                "Expiration is required.");
        }

        if (request.ExpiresAt <= now)
        {
            return CreateShortLinkResult.Failure(
                ShortLinkErrorCodes.InvalidExpiration,
                "Expiration must be in the future.");
        }

        var code = await GenerateUniqueCodeAsync(cancellationToken).ConfigureAwait(false);
        if (code is null)
        {
            return CreateShortLinkResult.Failure(
                ShortLinkErrorCodes.UnableToGenerateCode,
                "A unique short code could not be generated.");
        }

        var shortLink = new ShortLink(code, originalUrl, now, request.ExpiresAt.Value);
        await repository.AddAsync(shortLink, cancellationToken).ConfigureAwait(false);

        return CreateShortLinkResult.Success(shortLink);
    }

    public async Task<ResolveShortLinkResult> ResolveAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var validationFailure = ValidateCode(code);
        if (validationFailure is not null)
        {
            return ResolveShortLinkResult.Failure(validationFailure.Value.ErrorCode, validationFailure.Value.ErrorMessage);
        }

        var normalizedCode = code.Trim();
        var now = timeProvider.GetUtcNow();
        var shortLink = await cache.FindByCodeAsync(normalizedCode, cancellationToken).ConfigureAwait(false);
        if (shortLink is not null)
        {
            return await ResolveCachedAsync(shortLink, now, cancellationToken).ConfigureAwait(false);
        }

        shortLink = await repository.FindByCodeAsync(normalizedCode, cancellationToken).ConfigureAwait(false);
        if (shortLink is null)
        {
            return ResolveShortLinkResult.Failure(ShortLinkErrorCodes.NotFound, "Short link was not found.");
        }

        if (!shortLink.IsActive)
        {
            return ResolveShortLinkResult.Failure(ShortLinkErrorCodes.Inactive, "Short link is inactive.");
        }

        if (shortLink.IsExpired(now))
        {
            return ResolveShortLinkResult.Failure(ShortLinkErrorCodes.Expired, "Short link has expired.");
        }

        await cache.SetAsync(shortLink, cancellationToken).ConfigureAwait(false);

        return ResolveShortLinkResult.Success(shortLink);
    }

    public async Task<ShortLinkDetailsResult> GetDetailsAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var validationFailure = ValidateCode(code);
        if (validationFailure is not null)
        {
            return ShortLinkDetailsResult.Failure(validationFailure.Value.ErrorCode, validationFailure.Value.ErrorMessage);
        }

        var shortLink = await repository.FindByCodeAsync(code.Trim(), cancellationToken).ConfigureAwait(false);
        return shortLink is null
            ? ShortLinkDetailsResult.Failure(ShortLinkErrorCodes.NotFound, "Short link was not found.")
            : ShortLinkDetailsResult.Success(shortLink);
    }

    public async Task<DeactivateShortLinkResult> DeactivateAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var validationFailure = ValidateCode(code);
        if (validationFailure is not null)
        {
            return DeactivateShortLinkResult.Failure(validationFailure.Value.ErrorCode, validationFailure.Value.ErrorMessage);
        }

        var shortLink = await repository.FindByCodeAsync(code.Trim(), cancellationToken).ConfigureAwait(false);
        if (shortLink is null)
        {
            return DeactivateShortLinkResult.Failure(ShortLinkErrorCodes.NotFound, "Short link was not found.");
        }

        shortLink.Deactivate();
        await repository.UpdateAsync(shortLink, cancellationToken).ConfigureAwait(false);
        await cache.RemoveAsync(shortLink.Code, cancellationToken).ConfigureAwait(false);

        return DeactivateShortLinkResult.Success();
    }

    public async Task<DeactivateShortLinkResult> ActivateAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var validationFailure = ValidateCode(code);
        if (validationFailure is not null)
        {
            return DeactivateShortLinkResult.Failure(validationFailure.Value.ErrorCode, validationFailure.Value.ErrorMessage);
        }

        var shortLink = await repository.FindByCodeAsync(code.Trim(), cancellationToken).ConfigureAwait(false);
        if (shortLink is null)
        {
            return DeactivateShortLinkResult.Failure(ShortLinkErrorCodes.NotFound, "Short link was not found.");
        }

        shortLink.Activate();
        await repository.UpdateAsync(shortLink, cancellationToken).ConfigureAwait(false);
        await cache.RemoveAsync(shortLink.Code, cancellationToken).ConfigureAwait(false);

        return DeactivateShortLinkResult.Success();
    }

    public async Task<ShortLinkDetailsResult> UpdateAsync(
        string code,
        UpdateShortLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationFailure = ValidateCode(code);
        if (validationFailure is not null)
        {
            return ShortLinkDetailsResult.Failure(validationFailure.Value.ErrorCode, validationFailure.Value.ErrorMessage);
        }

        if (!ShortLinkUrlValidator.TryCreate(request.OriginalUrl, out var originalUrl))
        {
            return ShortLinkDetailsResult.Failure(
                ShortLinkErrorCodes.InvalidUrl,
                "Original URL must be an absolute HTTP or HTTPS URL.");
        }

        var now = timeProvider.GetUtcNow();
        if (request.ExpiresAt is null)
        {
            return ShortLinkDetailsResult.Failure(
                ShortLinkErrorCodes.InvalidExpiration,
                "Expiration is required.");
        }

        if (request.ExpiresAt <= now)
        {
            return ShortLinkDetailsResult.Failure(
                ShortLinkErrorCodes.InvalidExpiration,
                "Expiration must be in the future.");
        }

        var existing = await repository.FindByCodeAsync(code.Trim(), cancellationToken).ConfigureAwait(false);
        if (existing is null)
        {
            return ShortLinkDetailsResult.Failure(ShortLinkErrorCodes.NotFound, "Short link was not found.");
        }

        var updated = new ShortLink(
            existing.Code,
            originalUrl,
            existing.CreatedAt,
            request.ExpiresAt.Value,
            existing.IsActive);

        await repository.UpdateAsync(updated, cancellationToken).ConfigureAwait(false);
        await cache.RemoveAsync(updated.Code, cancellationToken).ConfigureAwait(false);

        return ShortLinkDetailsResult.Success(updated);
    }

    public async Task<DeactivateShortLinkResult> DeleteAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var validationFailure = ValidateCode(code);
        if (validationFailure is not null)
        {
            return DeactivateShortLinkResult.Failure(validationFailure.Value.ErrorCode, validationFailure.Value.ErrorMessage);
        }

        var normalizedCode = code.Trim();
        if (!await repository.ExistsByCodeAsync(normalizedCode, cancellationToken).ConfigureAwait(false))
        {
            return DeactivateShortLinkResult.Failure(ShortLinkErrorCodes.NotFound, "Short link was not found.");
        }

        await repository.DeleteAsync(normalizedCode, cancellationToken).ConfigureAwait(false);
        await cache.RemoveAsync(normalizedCode, cancellationToken).ConfigureAwait(false);

        return DeactivateShortLinkResult.Success();
    }

    private async Task<ResolveShortLinkResult> ResolveCachedAsync(
        ShortLink shortLink,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (!shortLink.IsActive)
        {
            await cache.RemoveAsync(shortLink.Code, cancellationToken).ConfigureAwait(false);
            return ResolveShortLinkResult.Failure(ShortLinkErrorCodes.Inactive, "Short link is inactive.");
        }

        if (shortLink.IsExpired(now))
        {
            await cache.RemoveAsync(shortLink.Code, cancellationToken).ConfigureAwait(false);
            return ResolveShortLinkResult.Failure(ShortLinkErrorCodes.Expired, "Short link has expired.");
        }

        return ResolveShortLinkResult.Success(shortLink);
    }

    private async Task<string?> GenerateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxCodeGenerationAttempts; attempt++)
        {
            var candidate = codeGenerator.Generate();
            if (!ShortCodeValidator.IsValid(candidate))
            {
                continue;
            }

            if (!await repository.ExistsByCodeAsync(candidate, cancellationToken).ConfigureAwait(false))
            {
                return candidate;
            }
        }

        return null;
    }

    private static (string ErrorCode, string ErrorMessage)? ValidateCode(string code)
    {
        if (!ShortCodeValidator.IsValid(code?.Trim()))
        {
            return (
                ShortLinkErrorCodes.InvalidCode,
                "Code can contain only letters, numbers, underscores, and hyphens.");
        }

        return null;
    }
}
