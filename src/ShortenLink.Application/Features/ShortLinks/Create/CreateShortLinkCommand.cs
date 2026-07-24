using ShortenLink.Core.Contracts.Results;
using ShortenLink.Mediator;

namespace ShortenLink.Application.Features.ShortLinks.Create;

public sealed record CreateShortLinkCommand(
    string OriginalUrl,
    DateTimeOffset? ExpiresAt,
    string? CreatedByUserId,
    string? CreatedByDisplayName,
    string? CreatedByUsername) : IRequest<CreateShortLinkResult>;
