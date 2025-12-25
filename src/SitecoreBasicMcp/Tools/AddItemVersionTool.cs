using GraphQL;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using SitecoreBasicMcp.Authentication;
using System.ComponentModel;

namespace SitecoreBasicMcp.Tools;

[McpServerToolType]
public class AddItemVersionTool(IOptions<SitecoreSettings> options, SitecoreAuthenticationService authenticationService) : SitecoreAuthoringToolBase(options, authenticationService)
{
    private static readonly string _addItemVersionMutation = """
            mutation AddItemVersion($itemId: ID!, $language: String!) {
              addItemVersion(input: { itemId: $itemId, language: $language }) {
                item {
                  id: itemId
                  path
                  name
                  version
                }
              }
            }
            """;

    record CreateItemData(BasicItem Item);
    record CreateItemMutationResponse(CreateItemData CreateItem);

    [McpServerTool(Idempotent = false, ReadOnly = false, UseStructuredContent = true), Description("Add a new version to a Sitecore item by its path or id.")]
    public async Task<object> AddItemVersion(string pathOrId, string language, CancellationToken cancellationToken)
    {
        var client = await GetAuthoringClient(cancellationToken);
        var resolveItemIdResult = await ResolveItemId(pathOrId, language, client, cancellationToken);

        if (resolveItemIdResult.ErrorResult != null)
        {
            return resolveItemIdResult.ErrorResult;
        }

        var itemId = resolveItemIdResult.ItemId;
        var request = new GraphQLRequest(_addItemVersionMutation)
        {
            Variables = new
            {
                itemId,
                language,
            }
        };

        var response = await client.SendMutationAsync<CreateItemMutationResponse>(request, cancellationToken);

        if (response.Errors != null)
        {
            return ErrorResultFromGraphQL(response.Errors);
        }

        return response.Data.CreateItem.Item;
    }
}

