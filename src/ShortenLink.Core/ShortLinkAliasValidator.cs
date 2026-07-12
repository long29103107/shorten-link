namespace ShortenLink.Core;

public static class ShortLinkAliasValidator
{
    public static bool IsValid(string? alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            return false;
        }

        foreach (var character in alias)
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
