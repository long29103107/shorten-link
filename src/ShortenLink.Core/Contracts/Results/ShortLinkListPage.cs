using ShortenLink.Core.Domain;

namespace ShortenLink.Core.Contracts.Results;

public sealed record ShortLinkListPage(
    IReadOnlyList<ShortLink> Items,
    int TotalCount);
