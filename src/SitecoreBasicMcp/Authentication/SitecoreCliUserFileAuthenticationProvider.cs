using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace SitecoreBasicMcp.Authentication;

public class SitecoreCliUserFileAuthenticationProvider(IOptions<SitecoreSettings> options, ILogger<SitecoreCliUserFileAuthenticationProvider> logger) : IAuthenticationProvider
{
    private readonly string? _endpointName = options.Value.CliUserFileAuthentication.EndpointName;
    private readonly string? _sitecoreCliUserFilePath = options.Value.CliUserFileAuthentication.FilePath;

    public async ValueTask<BearerToken?> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (_sitecoreCliUserFilePath == null || _endpointName == null)
        {
            logger.LogInformation("Skipping provider {ProviderName}, not configured.", nameof(SitecoreCliUserFileAuthenticationProvider));

            return null;
        }

        if (!File.Exists(_sitecoreCliUserFilePath))
        {
            throw new FileNotFoundException("The Sitecore CLI user file was not found.", _sitecoreCliUserFilePath);
        }

        using var jsonStream = new FileStream(_sitecoreCliUserFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var config = await JsonSerializer.DeserializeAsync<SitecoreCliUserConfig>(jsonStream, options: JsonSerializerOptions.Web, cancellationToken: cancellationToken);

        if (config == null)
        {
            throw new SitecoreCliUserFileException("Error while deserializing Sitecore CLI user file.");
        }

        if (config.Endpoints == null)
        {
            throw new SitecoreCliUserFileException("User file endpoints was null.");
        }

        if (!config.Endpoints.TryGetValue(_endpointName, out var endpoint))
        {
            throw new SitecoreCliUserFileException($"Sitecore endpoint '{_endpointName}' not found in user file.");
        }

        if (!string.IsNullOrEmpty(endpoint.Ref))
        {
            var referencedEndpoint = config.Endpoints.Where(x => string.Equals(x.Key, endpoint.Ref, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Value;

            if (referencedEndpoint == null)
            {
                throw new SitecoreCliUserFileException($"Sitecore reference endpoint '{endpoint.Ref}' not found in user file.");
            }

            endpoint = referencedEndpoint;
        }

        if (string.IsNullOrWhiteSpace(endpoint.AccessToken))
        {
            throw new SitecoreCliUserFileException("Access token was empty or whitespace.");
        }

        var token = new BearerToken(endpoint.AccessToken, endpoint.LastUpdated.AddSeconds(endpoint.ExpiresIn));

        if (!token.IsValid())
        {
            logger.LogWarning("Access token from Sitecore CLI user file is expired, run \"dotnet sitecore cloud login\" if you wish to use it.");
        }

        return token;
    }

    record SitecoreCliUserConfig(Dictionary<string, Endpoint> Endpoints);

    record Endpoint(string? Host, string? Ref, string? AccessToken, DateTimeOffset LastUpdated, int ExpiresIn);
}


[Serializable]
public class SitecoreCliUserFileException(string message) : Exception(message)
{
}
