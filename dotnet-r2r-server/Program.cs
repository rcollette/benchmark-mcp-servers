using McpDotnetServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new()
        {
            Name = "BenchmarkDotnetR2rServer",
            Version = "1.0.0"
        };
    })
    .WithHttpTransport()
    .WithTools<BenchmarkTools>();

var app = builder.Build();

app.MapGet("/health", () => Results.Text("""{"status":"ok","server_type":"dotnet-r2r"}""", "application/json"));

app.MapMcp("/mcp");

Console.WriteLine(".NET ReadyToRun MCP server listening on port 8086");
Console.WriteLine("MCP endpoint: http://localhost:8086/mcp");

app.Run();

