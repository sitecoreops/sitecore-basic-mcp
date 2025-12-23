using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace SitecoreBasicMcp.Authentication;

public class SitecoreCloudAuthenticationProvider(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<SitecoreCloudAuthenticationProvider> logger) : IAuthenticationProvider
{
    private readonly ConcurrentDictionary<string, BearerToken> _tokens = new();
    private readonly Uri _authenticationTokenUri = new("https://auth.sitecorecloud.io/oauth/token");
    private readonly string? _clientId = configuration["Sitecore:CloudAuthentication:ClientId"];
    private readonly string? _clientSecret = configuration["Sitecore:CloudAuthentication:ClientSecret"];

    public async ValueTask<BearerToken?> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (_clientId == null || _clientSecret == null)
        {
            logger.LogInformation("Skipping provider {ProviderName}, not configured.", nameof(SitecoreCloudAuthenticationProvider));

            return null;
        }

        using var client = httpClientFactory.CreateClient();
        using var requestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = _authenticationTokenUri,
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["audience"] = "https://api.sitecorecloud.io",
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
            })
        };

        using var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new SitecoreCloudAuthenticationException("Failure during authentication request.");
        }

        using var jsonStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var authResponse = await JsonSerializer.DeserializeAsync<AuthResponse>(jsonStream, cancellationToken: cancellationToken);

        if (authResponse == null)
        {
            throw new SitecoreCloudAuthenticationException("Could not deserialize authentication response.");
        }

        return new BearerToken(authResponse.access_token, DateTimeOffset.UtcNow.AddSeconds(authResponse.expires_in));
    }

    private record AuthResponse(int expires_in, string access_token);
}

[Serializable]
public class SitecoreCloudAuthenticationException(string message) : Exception(message)
{
}