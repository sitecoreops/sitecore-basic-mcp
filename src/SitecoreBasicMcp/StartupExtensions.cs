using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using SitecoreBasicMcp.Authentication;

namespace SitecoreBasicMcp;

public static class StartupExtensions
{
    public static IMcpServerBuilder AddSitecoreMcpServer(this IServiceCollection services)
    {
        var serverBuilder = services.AddSingleton<IAuthenticationProvider, SitecoreCliUserFileAuthenticationProvider>()
            .AddSingleton<IAuthenticationProvider, SitecoreCloudAuthenticationProvider>()
            .AddSingleton<SitecoreAuthenticationService>()
            .AddHttpClient()
            .AddMcpServer()
            .WithToolsFromAssembly(typeof(AssemblyMarker).Assembly)
            .WithPromptsFromAssembly(typeof(AssemblyMarker).Assembly)
            .AddCallToolFilter(next => async (context, cancellationToken) =>
            {
                try
                {
                    return await next(context, cancellationToken);
                }
                catch (Exception ex)
                {
                    return new CallToolResult
                    {
                        Content = [new TextContentBlock() { Text = $"Error: {ex.Message}" }],
                        IsError = true
                    };
                }
            });

        return serverBuilder;
    }
}
