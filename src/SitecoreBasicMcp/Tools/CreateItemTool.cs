using GraphQL;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using SitecoreBasicMcp.Authentication;
using System.ComponentModel;

namespace SitecoreBasicMcp.Tools;

[McpServerToolType]
public class CreateItemTool(IOptions<SitecoreSettings> options, SitecoreAuthenticationService authenticationService) : SitecoreAuthoringToolBase(options, authenticationService)
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
                  version
                }
              }
            }
            """;

    record CreateItemData(BasicItem Item);
    record CreateItemMutationResponse(CreateItemData CreateItem);

    [McpServerTool(Idempotent = false, ReadOnly = false, UseStructuredContent = true), Description("Create a new Sitecore item under parent id or path.")]
    public async Task<object> CreateItem(string parentPathOrId, string name, string templateId, string language, Field[] fields, CancellationToken cancellationToken)
    {
        var client = await GetAuthoringClient(cancellationToken);
        var resolveItemIdResult = await ResolveItemId(parentPathOrId, language, client, cancellationToken);

        if (resolveItemIdResult.GraphQLErrors != null)
        {
            return ErrorResultFromGraphQL(resolveItemIdResult.GraphQLErrors);
        }

        if (resolveItemIdResult.ErrorMessage == null)
        {
            return ErrorResult("Parent id could not be resolved.");
        }

        var parentId = resolveItemIdResult.ItemId;
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

