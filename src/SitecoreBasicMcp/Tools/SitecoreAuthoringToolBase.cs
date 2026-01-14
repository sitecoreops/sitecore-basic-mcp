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
    record GetItemQueryResponse(BasicItem? Item);
    private readonly string? _authoringEndpointUrl = options.Value.AuthoringEndpoint;
    private static readonly string _getItemQuery = """
            query GetItem($path: String, $language: String!) {
              item(where: { path: $path, language: $language }) {
                id: itemId
                path
                name
              }
            }
            """;

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


    protected async Task<ResolveItemIdResults> ResolveItemId(string pathOrId, string language, GraphQLHttpClient client, CancellationToken cancellationToken)
    {
        string? itemId;

        if (Guid.TryParse(pathOrId, out _))
        {
            itemId = pathOrId;
        }
        else
        {
            var getItemRequestg = new GraphQLRequest(_getItemQuery)
            {
                Variables = new
                {
                    path = pathOrId,
                    language
                }
            };

            var getItemResponse = await client.SendQueryAsync<GetItemQueryResponse>(getItemRequestg, cancellationToken);

            if (getItemResponse.Errors != null)
            {
                return new ResolveItemIdResults(ErrorResult: ErrorResultFromGraphQL(getItemResponse.Errors));
            }

            var item = getItemResponse.Data.Item;

            if (item == null)
            {
                return new ResolveItemIdResults(ErrorResult: ErrorResult("Parent item was not found."));
            }

            itemId = getItemResponse?.Data?.Item?.Id;
        }

        return new ResolveItemIdResults(ItemId: itemId);
    }

    protected CallToolResult ErrorResultFromGraphQL(GraphQLError[] errors)
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

    protected CallToolResult ErrorResult(string text) => new()
    {
        Content = [new TextContentBlock { Text = text }],
        IsError = true
    };

    protected CallToolResult ItemResult<T>(T item) where T : BasicItem
    {
        var node = JsonSerializer.SerializeToNode(item);

        if (node == null)
        {
            return ErrorResult("Node was null, item could not be serialized.");
        }

        return new()
        {

            Content = [new TextContentBlock { Text = node.ToJsonString() }],
        };
    }
}

public record ResolveItemIdResults(string? ItemId = null, CallToolResult? ErrorResult = null);
