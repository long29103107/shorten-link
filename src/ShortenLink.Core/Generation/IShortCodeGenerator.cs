namespace ShortenLink.Core.Generation;

public interface IShortCodeGenerator
{
    string Generate(int length = Base62ShortCodeGenerator.DefaultCodeLength);
}
