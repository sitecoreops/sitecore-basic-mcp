namespace SitecoreBasicMcp.Authentication;

public interface IAuthenticationProvider
{
    ValueTask<BearerToken?> GetTokenAsync(CancellationToken cancellationToken);
}
