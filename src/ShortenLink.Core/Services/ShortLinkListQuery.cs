using ShortenLink.Core.Domain;

namespace ShortenLink.Core.Services;

public sealed record ShortLinkListQuery(
    string? Search,
    ShortLinkListStatus Status,
    ShortLinkListSortBy SortBy,
    ShortLinkSortDirection SortDirection,
    DateTimeOffset Now,
    DateTimeOffset ExpiringSoonBefore);

public sealed record ShortLinkListPage(
    IReadOnlyList<ShortLink> Items,
    int TotalCount);

public enum ShortLinkListStatus
{
    All,
    Active,
    Inactive,
    Expired,
    ExpiringSoon
}

public enum ShortLinkListSortBy
{
    Created,
    Expiry,
    Destination,
    Code,
    Status
}

public enum ShortLinkSortDirection
{
    Asc,
    Desc
}
