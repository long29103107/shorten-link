namespace ShortenLink.Core;

public static class ShortCodeValidator
{
    public static bool IsValid(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        foreach (var character in code)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not '_' and not '-')
            {
                return false;
            }
        }

        return true;
    }

    public static void ValidateCodeOrThrow(string code)
    {
        if (!IsValid(code))
        {
            throw new ArgumentException(
                "Code can contain only letters, numbers, underscores, and hyphens.",
                nameof(code));
        }
    }
}
