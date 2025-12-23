using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using SitecoreBasicMcp.Authentication;
using System.Text.Json;

namespace SitecoreBasicMcp.Tools;

public abstract class SitecoreAuthoringToolBase(IConfiguration configuration, SitecoreAuthenticationService authenticationService)
{
    private readonly string _authoringEndpointUrl = configuration["Sitecore:AuthoringEndpoint"] ?? throw new ArgumentNullException("Sitecore:AuthoringEndpoint");

    protected async Task<GraphQLHttpClient> GetAuthoringClient(CancellationToken cancellationToken)
    {
        var token = await authenticationService.GetTokenAsync(cancellationToken);
        var authoringClient = new GraphQLHttpClient(_authoringEndpointUrl, new SystemTextJsonSerializer(new JsonSerializerOptions(McpJsonUtilities.DefaultOptions)));

        authoringClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.Token}");

        return authoringClient;
    }

    protected static CallToolResult ErrorResultFromGraphQL(GraphQLError[] errors)
    {
        var errorContent = new List<ContentBlock>();

        foreach (var error in errors)
        {
            errorContent.Add(new TextContentBlock { Text = error.Message });
        }

        return new CallToolResult
        {
            Content = errorContent,
            IsError = true
        };
    }

    protected static CallToolResult ErrorResult(string text) => new()
    {
        Content = [new TextContentBlock { Text = text }],
        IsError = true
    };
}

