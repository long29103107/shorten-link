namespace ShortenLink.Core.Services;

public interface IShortLinkService
{
    Task<CreateShortLinkResult> CreateAsync(
        CreateShortLinkRequest request,
        CancellationToken cancellationToken = default);

    Task<ResolveShortLinkResult> ResolveAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<ShortLinkDetailsResult> GetDetailsAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<DeactivateShortLinkResult> DeactivateAsync(
        string code,
        CancellationToken cancellationToken = default);
}
