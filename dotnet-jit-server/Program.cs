using McpDotnetServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new()
        {
            Name = "BenchmarkDotnetJitServer",
            Version = "1.0.0"
        };
    })
    .WithHttpTransport()
    .WithTools<BenchmarkTools>();

var app = builder.Build();

app.MapGet("/health", () => Results.Text("""{"status":"ok","server_type":"dotnet-jit"}""", "application/json"));

app.MapMcp("/mcp");

Console.WriteLine(".NET JIT MCP server listening on port 8085");
Console.WriteLine("MCP endpoint: http://localhost:8085/mcp");

app.Run();

