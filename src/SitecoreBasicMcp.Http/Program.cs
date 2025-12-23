using SitecoreBasicMcp;

var builder = WebApplication.CreateBuilder(args);

builder.AddSitecoreMcpServer()
    .WithHttpTransport();

var app = builder.Build();

app.MapMcp();
app.Run();
