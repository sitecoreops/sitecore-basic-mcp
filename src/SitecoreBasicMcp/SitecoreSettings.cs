namespace SitecoreBasicMcp;

public class SitecoreSettings
{
    public static readonly string Key = "Sitecore";

    public string? AuthoringEndpoint { get; set; } = "https://xmcloudcm.localhost/sitecore/api/authoring/graphql/v1";

    public CliUserFileAuthenticationSettings CliUserFileAuthentication { get; set; } = new CliUserFileAuthenticationSettings();

    public CloudAuthenticationSettings CloudAuthentication { get; set; } = new CloudAuthenticationSettings();

    public class CliUserFileAuthenticationSettings
    {
        public string? EndpointName { get; set; } = "default";
        public string? FilePath { get; set; }
    }

    public class CloudAuthenticationSettings
    {
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
    }
}