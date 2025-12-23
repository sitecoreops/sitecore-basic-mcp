# Contributing to Sitecore Basic MCP

## Prerequisites

- .NET 10 SDK

## Testing

1. List tools: `curl -vs http://localhost:9001 -H "Content-Type: application/json" -d '{"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}'`
1. Get item: `curl -vs http://localhost:9001 -H "Content-Type: application/json" -d '{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name": "get_item", "arguments": {"pathOrId": "/sitecore/content/home"}}}'`
