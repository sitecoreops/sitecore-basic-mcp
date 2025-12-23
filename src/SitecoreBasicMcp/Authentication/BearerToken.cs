namespace SitecoreBasicMcp.Authentication;

public class BearerToken(string token, DateTimeOffset expiresUtc)
{
    public string Token { get; } = token;

    public bool IsValid()
    {
        return DateTimeOffset.UtcNow < expiresUtc;
    }
}