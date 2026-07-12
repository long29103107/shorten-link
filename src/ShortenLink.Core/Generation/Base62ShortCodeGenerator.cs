using System.Security.Cryptography;

namespace ShortenLink.Core.Generation;

public sealed class Base62ShortCodeGenerator : IShortCodeGenerator
{
    public const int DefaultCodeLength = 7;
    public const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public string Generate(int length = DefaultCodeLength)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), length, "Code length must be greater than zero.");
        }

        return string.Create(length, Alphabet, static (buffer, alphabet) =>
        {
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
            }
        });
    }
}
