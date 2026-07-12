using ShortenLink.Core.Domain;
using ShortenLink.Core.Generation;
using ShortenLink.Core.Repositories;

namespace ShortenLink.Core.Services;

public sealed class ShortLinkService : IShortLinkService
{
    private const int MaxCodeGenerationAttempts = 10;

    private readonly IShortLinkRepository repository;
    private readonly IShortCodeGenerator codeGenerator;
    private readonly TimeProvider timeProvider;

    public ShortLinkService(
        IShortLinkRepository repository,
        IShortCodeGenerator codeGenerator,
        TimeProvider? timeProvider = null)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.codeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

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
        if (request.ExpiresAt is not null && request.ExpiresAt <= now)
        {
            return CreateShortLinkResult.Failure(
                ShortLinkErrorCodes.InvalidExpiration,
                "Expiration must be in the future.");
        }

        var code = request.CustomAlias?.Trim();
        if (!string.IsNullOrEmpty(code))
        {
            if (!ShortLinkAliasValidator.IsValid(code))
            {
                return CreateShortLinkResult.Failure(
                    ShortLinkErrorCodes.InvalidAlias,
                    "Custom alias can contain only letters, numbers, underscores, and hyphens.");
            }

            if (await repository.ExistsByCodeAsync(code, cancellationToken).ConfigureAwait(false))
            {
                return CreateShortLinkResult.Failure(
                    ShortLinkErrorCodes.DuplicateAlias,
                    "Custom alias is already in use.");
            }
        }
        else
        {
            code = await GenerateUniqueCodeAsync(cancellationToken).ConfigureAwait(false);
            if (code is null)
            {
                return CreateShortLinkResult.Failure(
                    ShortLinkErrorCodes.UnableToGenerateCode,
                    "A unique short code could not be generated.");
            }
        }

        var shortLink = new ShortLink(code, originalUrl, now, request.ExpiresAt);
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

        var shortLink = await repository.FindByCodeAsync(code.Trim(), cancellationToken).ConfigureAwait(false);
        if (shortLink is null)
        {
            return ResolveShortLinkResult.Failure(ShortLinkErrorCodes.NotFound, "Short link was not found.");
        }

        if (!shortLink.IsActive)
        {
            return ResolveShortLinkResult.Failure(ShortLinkErrorCodes.Inactive, "Short link is inactive.");
        }

        if (shortLink.IsExpired(timeProvider.GetUtcNow()))
        {
            return ResolveShortLinkResult.Failure(ShortLinkErrorCodes.Expired, "Short link has expired.");
        }

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

        return DeactivateShortLinkResult.Success();
    }

    private async Task<string?> GenerateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxCodeGenerationAttempts; attempt++)
        {
            var candidate = codeGenerator.Generate();
            if (!ShortLinkAliasValidator.IsValid(candidate))
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
        if (!ShortLinkAliasValidator.IsValid(code?.Trim()))
        {
            return (
                ShortLinkErrorCodes.InvalidCode,
                "Code can contain only letters, numbers, underscores, and hyphens.");
        }

        return null;
    }
}
