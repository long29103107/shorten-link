namespace ShortenLink.Core.Services;

public static class ShortLinkErrorCodes
{
    public const string DuplicateAlias = "duplicate_alias";
    public const string Expired = "expired";
    public const string Inactive = "inactive";
    public const string InvalidAlias = "invalid_alias";
    public const string InvalidCode = "invalid_code";
    public const string InvalidExpiration = "invalid_expiration";
    public const string InvalidUrl = "invalid_url";
    public const string NotFound = "not_found";
    public const string UnableToGenerateCode = "unable_to_generate_code";
}
