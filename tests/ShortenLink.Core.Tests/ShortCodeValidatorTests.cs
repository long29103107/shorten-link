using Xunit;

namespace ShortenLink.Core.Tests;

public sealed class ShortCodeValidatorTests
{
    [Theory]
    [InlineData("abc")]
    [InlineData("ABC")]
    [InlineData("abc123")]
    [InlineData("a_b-c")]
    public void IsValid_AcceptsLettersNumbersUnderscoreAndHyphen(string code)
    {
        Assert.True(ShortCodeValidator.IsValid(code));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc.def")]
    [InlineData("abc/def")]
    [InlineData("abc def")]
    [InlineData("abc@def")]
    public void IsValid_RejectsMissingOrUnsupportedCharacters(string? code)
    {
        Assert.False(ShortCodeValidator.IsValid(code));
    }
}
