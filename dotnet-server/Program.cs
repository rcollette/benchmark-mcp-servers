using McpDotnetServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new()
        {
            Name = "BenchmarkDotnetServer",
            Version = "1.0.0"
        };
    })
    .WithHttpTransport()
    .WithTools<BenchmarkTools>();

var app = builder.Build();

app.MapGet("/health", () => Results.Text("""{"status":"ok","server_type":"dotnet"}""", "application/json"));

app.MapMcp("/mcp");

Console.WriteLine(".NET MCP server listening on port 8084");
Console.WriteLine("MCP endpoint: http://localhost:8084/mcp");

app.Run();
