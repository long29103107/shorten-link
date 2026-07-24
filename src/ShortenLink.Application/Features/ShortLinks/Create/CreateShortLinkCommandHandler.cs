using ShortenLink.Core.Abstractions;
using ShortenLink.Core.Contracts.Requests;
using ShortenLink.Core.Contracts.Results;
using ShortenLink.Mediator;

namespace ShortenLink.Application.Features.ShortLinks.Create;

internal sealed class CreateShortLinkCommandHandler(IShortLinkService shortLinkService)
    : IRequestHandler<CreateShortLinkCommand, CreateShortLinkResult>
{
    public Task<CreateShortLinkResult> Handle(
        CreateShortLinkCommand request,
        CancellationToken cancellationToken) =>
        shortLinkService.CreateAsync(
            new CreateShortLinkRequest(
                request.OriginalUrl,
                request.ExpiresAt,
                request.CreatedByUserId,
                request.CreatedByDisplayName,
                request.CreatedByUsername),
            cancellationToken);
}
