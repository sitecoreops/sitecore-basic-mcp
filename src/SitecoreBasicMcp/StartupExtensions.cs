using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Protocol;
using SitecoreBasicMcp.Authentication;
using SitecoreBasicMcp.Tools;

namespace SitecoreBasicMcp;

public static class StartupExtensions
{
    public static IMcpServerBuilder AddSitecoreMcpServer(this IHostApplicationBuilder builder)
    {
        var sitecoreSection = builder.Configuration.GetSection(SitecoreSettings.Key);
        var sitecoreSettings = sitecoreSection.Get<SitecoreSettings>();

        ArgumentNullException.ThrowIfNull(sitecoreSettings, nameof(sitecoreSettings));

        builder.Services.Configure<SitecoreSettings>(sitecoreSection);
        builder.Services.AddSingleton<IAuthenticationProvider, SitecoreCliUserFileAuthenticationProvider>();
        builder.Services.AddSingleton<IAuthenticationProvider, SitecoreCloudAuthenticationProvider>();
        builder.Services.AddSingleton<SitecoreAuthenticationService>();

        var serverBuilder = builder.Services
            .AddHttpClient()
            .AddMcpServer()
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

        if (sitecoreSettings.ReadonlyMode)
        {
            serverBuilder.WithTools<GetItemTool>();
        }
        else
        {
            serverBuilder.WithToolsFromAssembly(typeof(AssemblyMarker).Assembly);
        }

        return serverBuilder;
    }
}
