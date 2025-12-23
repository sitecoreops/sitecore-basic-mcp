# Sitecore Basic MCP

Minimalistic CRUD only MCP Server for SitecoreAI (local and remote instances).

When you have configured your MCP client, try out these prompts:

```text
"Show me a key value list of fields on the /sitecore/content/home item"
"Create a new item named test42 under /sitecore/content/home using template
76036F5E-CBCE-46D1-AF0A-4143F9B557AA and set the title field to Just testing"
"Update the item /sitecore/content/home/test42, set the title field to Even more testing and the text field to Hello MCP"
"Delete the item /sitecore/content/home/test42"
```

> TIP: If you want more advanced features, then you should take a look at: [https://github.com/Antonytm/mcp-sitecore-server](https://github.com/Antonytm/mcp-sitecore-server).

## Server configuration

At least _one_ authentication provider must be configured, executed in order:

- `CliUserFileAuthentication`, point to the `user.json` file created by `dotnet sitecore cloud login`
- `CloudAuthentication`, create and use client id and secret from [SitecoreAI Deploy](https://deploy.sitecorecloud.io/) organization credentials.

| Name                                            | Default                                                         | Description                              | 
| ----------------------------------------------- | --------------------------------------------------------------- | ---------------------------------------- | 
| Sitecore:AuthoringEndpoint                      | `https://xmcloudcm.localhost/sitecore/api/authoring/graphql/v1` | Url to the authoring endpoint            | 
| Sitecore:CliUserFileAuthentication:FilePath     |                                                                 | Path to the Sitecore CLI user file       | 
| Sitecore:CliUserFileAuthentication:EndpointName | `default`                                                       | The endpoint name to use                 | 
| Sitecore:CloudAuthentication:ClientId           |                                                                 | Id with access to authoring endpoint     | 
| Sitecore:CloudAuthentication:ClientSecret       |                                                                 | Secret with access to authoring endpoint | 
