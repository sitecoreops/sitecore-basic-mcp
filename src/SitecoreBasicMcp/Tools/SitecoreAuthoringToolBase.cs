using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Options;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using SitecoreBasicMcp.Authentication;
using System.Text.Json;

namespace SitecoreBasicMcp.Tools;

public abstract class SitecoreAuthoringToolBase(IOptions<SitecoreSettings> options, SitecoreAuthenticationService authenticationService)
{
    private readonly string? _authoringEndpointUrl = options.Value.AuthoringEndpoint;

    protected async Task<GraphQLHttpClient> GetAuthoringClient(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_authoringEndpointUrl))
        {
            throw new Exception("AuthoringEndpoint is not configured.");
        }

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

