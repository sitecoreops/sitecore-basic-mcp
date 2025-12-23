using Microsoft.Extensions.Logging;

namespace SitecoreBasicMcp.Authentication;

public class SitecoreAuthenticationService(IEnumerable<IAuthenticationProvider> providers, ILogger<SitecoreAuthenticationService> logger)
{
    private BearerToken? _cachedToken;

    public async ValueTask<BearerToken> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (_cachedToken != null && _cachedToken.IsValid())
        {
            logger.LogInformation("Using cached token.");

            return _cachedToken;
        }

        foreach (var provider in providers)
        {
            logger.LogInformation("Attempting to get token from provider: {ProviderType}...", provider.GetType().Name);

            var token = await provider.GetTokenAsync(cancellationToken);

            if (token != null && token.IsValid())
            {
                logger.LogInformation("Got valid token!");

                _cachedToken = token;

                return token;
            }
        }

        throw new Exception("No authentication providers were able to provide a valid token.");
    }
}
