using GraphQL;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using SitecoreBasicMcp.Authentication;
using System.ComponentModel;

namespace SitecoreBasicMcp.Tools;

[McpServerToolType]
public class DeleteItemTool(IOptions<SitecoreSettings> options, SitecoreAuthenticationService authenticationService) : SitecoreAuthoringToolBase(options, authenticationService)
{
    private static readonly string _deleteItemMutation = """
            mutation DeleteItem($path: String!) {
              deleteItem( input: { 
                  path: $path
                  permanently: false
                }
              ) {
                successful
              }
            }
            """;

    record DeleteItemData(bool Successful);
    record DeleteItemMutationResponse(DeleteItemData DeleteItem);

    [McpServerTool(Idempotent = false, ReadOnly = false, UseStructuredContent = true), Description("Delete a Sitecore item by its path or id.")]
    public async Task<object> DeleteItem(string pathOrId, CancellationToken cancellationToken)
    {
        var client = await GetAuthoringClient(cancellationToken);
        var request = new GraphQLRequest(_deleteItemMutation)
        {
            Variables = new
            {
                path = pathOrId
            }
        };

        var response = await client.SendMutationAsync<DeleteItemMutationResponse>(request, cancellationToken);

        if (response.Errors != null)
        {
            return ErrorResultFromGraphQL(response.Errors);
        }

        return response.Data.DeleteItem.Successful;
    }
}
