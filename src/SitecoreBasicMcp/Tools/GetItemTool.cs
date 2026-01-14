using GraphQL;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using SitecoreBasicMcp.Authentication;
using System.ComponentModel;
using System.Text.Json;

namespace SitecoreBasicMcp.Tools;

[McpServerToolType]
public class GetItemTool(IOptions<SitecoreSettings> options, SitecoreAuthenticationService authenticationService) : SitecoreAuthoringToolBase(options, authenticationService)
{
    private static readonly string _getItemQuery = """
            fragment BasicItem on Item {
              id: itemId
              name
              path
              version
            }

            query GetItem($path: String, $language: String!) {
              item(where: { path: $path, language: $language }) {
                ...BasicItem
                template {
                  id: templateId
                  fullName
                  name
                }
                parent {
                  ...BasicItem
                }
                children {
                  nodes {
                    ...BasicItem
                  }
                }
                fields(ownFields: true) {
                  nodes {
                    name
                    value
                  }
                }
              }
            }
            """;

    record GetItemQueryResponse(Item? Item);

    [McpServerTool(Idempotent = true, ReadOnly = true), Description("Get a Sitecore item by its path or id.")]
    public async Task<CallToolResult> GetItem(string pathOrId, CancellationToken cancellationToken, string language = "en", bool includeStandardFields = false)
    {
        var client = await GetAuthoringClient(cancellationToken);
        var request = new GraphQLRequest(_getItemQuery)
        {
            Variables = new
            {
                path = pathOrId,
                language,
                ownFields = !includeStandardFields
            }
        };

        var response = await client.SendQueryAsync<GetItemQueryResponse>(request, cancellationToken);

        if (response.Errors != null)
        {
            return ErrorResultFromGraphQL(response.Errors);
        }

        var item = response.Data.Item;

        if (item == null)
        {
            return ErrorResult("Item was not found.");
        }

        return ItemResult(item);
    }
}
