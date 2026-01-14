using GraphQL;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using SitecoreBasicMcp.Authentication;
using System.ComponentModel;

namespace SitecoreBasicMcp.Tools;

[McpServerToolType]
public class UpdateItemTool(IOptions<SitecoreSettings> options, SitecoreAuthenticationService authenticationService) : SitecoreAuthoringToolBase(options, authenticationService)
{
    private static readonly string _updateItemMutation = """
            mutation UpdateItem(
              $path: String!
              $language: String!
              $fields: [FieldValueInput]!
            ) {
              updateItem(input: { path: $path, language: $language, fields: $fields }) {
                item {
                  id: itemId
                  path
                  name
                  version
                }
              }
            }
            """;

    record UpdateItemData(BasicItem Item);
    record UpdateItemMutationResponse(UpdateItemData UpdateItem);

    [McpServerTool(Idempotent = false, ReadOnly = false), Description("Update a Sitecore item by its path or id.")]
    public async Task<CallToolResult> UpdateItem(string pathOrId, string language, Field[] fields, CancellationToken cancellationToken)
    {
        var client = await GetAuthoringClient(cancellationToken);
        var request = new GraphQLRequest(_updateItemMutation)
        {
            Variables = new
            {
                path = pathOrId,
                language,
                fields
            }
        };

        var response = await client.SendMutationAsync<UpdateItemMutationResponse>(request, cancellationToken);

        if (response.Errors != null)
        {
            return ErrorResultFromGraphQL(response.Errors);
        }

        return ItemResult(response.Data.UpdateItem.Item);
    }
}
