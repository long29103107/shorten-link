namespace ShortenLink.Core.Services;

public interface IShortLinkClickRecorder
{
    Task RecordAsync(RecordShortLinkClickRequest request, CancellationToken cancellationToken = default);
}
