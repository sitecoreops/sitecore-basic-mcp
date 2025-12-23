using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Protocol;
using SitecoreBasicMcp.Authentication;

namespace SitecoreBasicMcp;

public static class StartupExtensions
{
    public static IMcpServerBuilder AddSitecoreMcpServer(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<SitecoreSettings>(builder.Configuration.GetSection(SitecoreSettings.Key));
        builder.Services.AddSingleton<IAuthenticationProvider, SitecoreCliUserFileAuthenticationProvider>();
        builder.Services.AddSingleton<IAuthenticationProvider, SitecoreCloudAuthenticationProvider>();
        builder.Services.AddSingleton<SitecoreAuthenticationService>();

        var serverBuilder = builder.Services
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
