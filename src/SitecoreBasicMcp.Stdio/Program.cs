using SitecoreBasicMcp;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.AddSitecoreMcpServer()
    .WithStdioServerTransport();

await builder.Build().RunAsync();
