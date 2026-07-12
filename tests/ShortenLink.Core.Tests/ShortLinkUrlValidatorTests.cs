using Xunit;

namespace ShortenLink.Core.Tests;

public sealed class ShortLinkUrlValidatorTests
{
    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com/path?x=1")]
    public void IsValid_AcceptsAbsoluteHttpAndHttpsUrls(string url)
    {
        Assert.True(ShortLinkUrlValidator.IsValid(url));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not-a-url")]
    [InlineData("/relative")]
    [InlineData("ftp://example.com/file")]
    [InlineData("mailto:test@example.com")]
    public void IsValid_RejectsMissingMalformedAndNonHttpUrls(string? url)
    {
        Assert.False(ShortLinkUrlValidator.IsValid(url));
    }
}
