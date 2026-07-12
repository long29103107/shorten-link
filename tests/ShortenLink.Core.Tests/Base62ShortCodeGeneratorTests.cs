using ShortenLink.Core.Generation;
using Xunit;

namespace ShortenLink.Core.Tests;

public sealed class Base62ShortCodeGeneratorTests
{
    [Fact]
    public void Generate_UsesDefaultLengthAndBase62Characters()
    {
        var generator = new Base62ShortCodeGenerator();

        var code = generator.Generate();

        Assert.Equal(Base62ShortCodeGenerator.DefaultCodeLength, code.Length);
        Assert.All(code, character => Assert.Contains(character, Base62ShortCodeGenerator.Alphabet));
    }

    [Fact]
    public void Generate_UsesRequestedLength()
    {
        var generator = new Base62ShortCodeGenerator();

        var code = generator.Generate(12);

        Assert.Equal(12, code.Length);
    }

    [Fact]
    public void Generate_RejectsNonPositiveLength()
    {
        var generator = new Base62ShortCodeGenerator();

        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(0));
    }
}
