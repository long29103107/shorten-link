namespace ShortenLink.Core;

public static class ShortLinkUrlValidator
{
    public static bool IsValid(string? url) => TryCreate(url, out _);

    public static bool TryCreate(string? url, out Uri uri)
    {
        uri = null!;

        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed))
        {
            return false;
        }

        if (!string.Equals(parsed.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(parsed.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        uri = parsed;
        return true;
    }
}
