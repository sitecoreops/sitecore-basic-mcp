using GraphQL;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using SitecoreBasicMcp.Authentication;
using System.ComponentModel;

namespace SitecoreBasicMcp.Tools;

[McpServerToolType]
public class DeleteItemVersionTool(IOptions<SitecoreSettings> options, SitecoreAuthenticationService authenticationService) : SitecoreAuthoringToolBase(options, authenticationService)
{
    private static readonly string _deleteItemMutation = """
            mutation DeleteVersionItem($itemId: ID!, $language: String!, $version: Int!) {
              deleteItemVersion(
                input: { itemId: $itemId, language: $language, version: $version }
              ) {
                item {
                  id: itemId
                  path
                  name
                  version
                }
              }
            }
            """;

    record DeleteItemVersionData(BasicItem Item);
    record DeleteItemMutationResponse(DeleteItemVersionData DeleteItemVersion);

    [McpServerTool(Idempotent = false, ReadOnly = false, UseStructuredContent = true), Description("Delete a language version on a Sitecore item by its path or id.")]
    public async Task<object> DeleteItemVersion(string pathOrId, string language, int version, CancellationToken cancellationToken)
    {
        var client = await GetAuthoringClient(cancellationToken);
        var resolveItemIdResult = await ResolveItemId(pathOrId, language, client, cancellationToken);

        if (resolveItemIdResult.ErrorResult != null)
        {
            return resolveItemIdResult.ErrorResult;
        }

        var itemId = resolveItemIdResult.ItemId;
        var request = new GraphQLRequest(_deleteItemMutation)
        {
            Variables = new
            {
                itemId,
                language,
                version
            }
        };

        var response = await client.SendMutationAsync<DeleteItemMutationResponse>(request, cancellationToken);

        if (response.Errors != null)
        {
            return ErrorResultFromGraphQL(response.Errors);
        }

        return response.Data.DeleteItemVersion.Item;
    }
}
