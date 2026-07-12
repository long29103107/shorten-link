using Xunit;

namespace ShortenLink.Core.Tests;

public sealed class ShortLinkAliasValidatorTests
{
    [Theory]
    [InlineData("abc")]
    [InlineData("ABC")]
    [InlineData("abc123")]
    [InlineData("a_b-c")]
    public void IsValid_AcceptsLettersNumbersUnderscoreAndHyphen(string alias)
    {
        Assert.True(ShortLinkAliasValidator.IsValid(alias));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc.def")]
    [InlineData("abc/def")]
    [InlineData("abc def")]
    [InlineData("abc@def")]
    public void IsValid_RejectsMissingOrUnsupportedCharacters(string? alias)
    {
        Assert.False(ShortLinkAliasValidator.IsValid(alias));
    }
}
