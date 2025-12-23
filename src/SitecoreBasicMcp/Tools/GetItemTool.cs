using GraphQL;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Server;
using SitecoreBasicMcp.Authentication;
using System.ComponentModel;

namespace SitecoreBasicMcp.Tools;

[McpServerToolType]
public class GetItemTool(IConfiguration configuration, SitecoreAuthenticationService authenticationService) : SitecoreAuthoringToolBase(configuration, authenticationService)
{
    private static readonly string _getItemQuery = """
            query GetItem($path: String, $language: String, $ownFields: Boolean) {
              item(where: { path: $path, language: $language }) {
                id: itemId
                path
                name
                template {
                  id: templateId
                  fullName
                  name
                }
                parent {
                  id: itemId
                  path
                  name
                }
                children {
                  nodes {
                    id: itemId
                    path
                    name
                  }
                }
                fields(ownFields:$ownFields) {
                  nodes {
                    name
                    value
                  }
                }
              }
            }
            """;

    record GetItemQueryResponse(Item? Item);

    [McpServerTool(Idempotent = true, ReadOnly = true, UseStructuredContent = true), Description("Get a Sitecore item by its path or id.")]
    public async Task<object> GetItem(string pathOrId, CancellationToken cancellationToken, string language = "en", bool includeStandardFields = false)
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

        return item;
    }
}
