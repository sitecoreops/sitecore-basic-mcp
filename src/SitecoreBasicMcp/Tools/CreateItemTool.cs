using GraphQL;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Server;
using SitecoreBasicMcp.Authentication;
using System.ComponentModel;

namespace SitecoreBasicMcp.Tools;

[McpServerToolType]
public class CreateItemTool(IConfiguration configuration, SitecoreAuthenticationService authenticationService) : SitecoreAuthoringToolBase(configuration, authenticationService)
{
    private static readonly string _createItemMutation = """
            mutation CreateItem(
              $name: String!
              $parentId: ID!
              $language: String!
              $templateId: ID!
              $fields: [FieldValueInput]
            ) {
              createItem(
                input: {
                  name: $name
                  templateId: $templateId
                  parent: $parentId
                  language: $language
                  fields: $fields
                }
              ) {
                item {
                  id: itemId
                  path
                  name
                }
              }
            }
            """;
    private static readonly string _getItemQuery = """
            query GetItem($path: String, $language: String!) {
              item(where: { path: $path, language: $language }) {
                id: itemId
                path
                name
              }
            }
            """;

    record CreateItemData(BasicItem Item);
    record CreateItemMutationResponse(CreateItemData CreateItem);
    record GetItemQueryResponse(BasicItem? Item);

    [McpServerTool(Idempotent = false, ReadOnly = false, UseStructuredContent = true), Description("Create a new Sitecore item under parent id or path.")]
    public async Task<object> CreateItem(string parentPathOrId, string name, string templateId, string language, Field[] fields, CancellationToken cancellationToken)
    {
        var client = await GetAuthoringClient(cancellationToken);
        string parentId;

        if (Guid.TryParse(parentPathOrId, out _))
        {
            parentId = parentPathOrId;
        }
        else
        {
            var getParentRequest = new GraphQLRequest(_getItemQuery)
            {
                Variables = new
                {
                    path = parentPathOrId,
                    language
                }
            };

            var gerParentResponse = await client.SendQueryAsync<GetItemQueryResponse>(getParentRequest, cancellationToken);

            if (gerParentResponse.Errors != null)
            {
                return ErrorResultFromGraphQL(gerParentResponse.Errors);
            }

            var parentItem = gerParentResponse.Data.Item;

            if (parentItem == null)
            {
                return ErrorResult("Parent item was not found.");
            }

            parentId = parentItem.Id;
        }

        var request = new GraphQLRequest(_createItemMutation)
        {
            Variables = new
            {
                name,
                parentId,
                language,
                templateId,
                fields
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

