namespace ShortenLink.Core.Services;

public interface IShortLinkService
{
    Task<IReadOnlyList<Domain.ShortLink>> ListRecentAsync(
        int limit = 100,
        CancellationToken cancellationToken = default);

    Task<CreateShortLinkResult> CreateAsync(
        CreateShortLinkRequest request,
        CancellationToken cancellationToken = default);

    Task<ResolveShortLinkResult> ResolveAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<ShortLinkDetailsResult> GetDetailsAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<ShortLinkDetailsResult> UpdateAsync(
        string code,
        UpdateShortLinkRequest request,
        CancellationToken cancellationToken = default);

    Task<DeactivateShortLinkResult> DeactivateAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<DeactivateShortLinkResult> ActivateAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<DeactivateShortLinkResult> DeleteAsync(
        string code,
        CancellationToken cancellationToken = default);
}
