using System.Buffers;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol.Server;

namespace McpDotnetServer;

// ── Source-generated JSON serialization (AOT-friendly, zero-reflection) ──

[JsonSerializable(typeof(FibonacciResult))]
[JsonSerializable(typeof(FetchResult))]
[JsonSerializable(typeof(FetchErrorResult))]
[JsonSerializable(typeof(DbQueryResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
internal partial class BenchmarkJsonContext : JsonSerializerContext;

// ── Readonly DTOs ──

internal readonly record struct FibonacciResult(int Input, long Result, string ServerType);
internal readonly record struct FetchResult(string Url, int StatusCode, long ResponseTimeMs, string ServerType);
internal readonly record struct FetchErrorResult(string Url, int StatusCode, long ResponseTimeMs, string Error, string ServerType);
internal readonly record struct DbQueryResult(string Query, int DelayMs, string Timestamp, string ServerType);

// ── Tools ──

[McpServerToolType]
public sealed class BenchmarkTools
{
    private static readonly string ServerType = Environment.GetEnvironmentVariable("SERVER_TYPE") ?? "dotnet";

    // ── Recursive Fibonacci (matches Go/Java implementations) ──

    private static long Fibonacci(int n)
    {
        if (n <= 1)
            return n;
        return Fibonacci(n - 1) + Fibonacci(n - 2);
    }

    [McpServerTool(Name = "calculate_fibonacci"), Description("Calculates the Nth Fibonacci number")]
    public static string CalculateFibonacci([Description("Integer between 0 and 40")] int n)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(n, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(n, 40);

        return JsonSerializer.Serialize(
            new FibonacciResult(n, Fibonacci(n), ServerType),
            BenchmarkJsonContext.Default.FibonacciResult);
    }

    // ── HTTP fetch — ResponseHeadersRead avoids buffering the response body ──

    [McpServerTool(Name = "fetch_external_data"), Description("Makes an HTTP GET request to an external API")]
    public static async Task<string> FetchExternalData(
        [Description("Full endpoint URL")] string endpoint,
        IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient();
        var sw = Stopwatch.StartNew();

        try
        {
            using var response = await client
                .GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);
            sw.Stop();

            return JsonSerializer.Serialize(
                new FetchResult(endpoint, (int)response.StatusCode, sw.ElapsedMilliseconds, ServerType),
                BenchmarkJsonContext.Default.FetchResult);
        }
        catch (Exception e)
        {
            sw.Stop();

            return JsonSerializer.Serialize(
                new FetchErrorResult(endpoint, 0, sw.ElapsedMilliseconds, e.Message, ServerType),
                BenchmarkJsonContext.Default.FetchErrorResult);
        }
    }

    // ── JSON transform — single-pass Utf8JsonWriter for the entire response envelope ──

    [McpServerTool(Name = "process_json_data"), Description("Receives JSON, validates, and transforms (uppercases string fields)")]
    public static string ProcessJsonData(
        [Description("JSON object to process")] JsonElement data)
    {
        var buffer = new ArrayBufferWriter<byte>(256);
        using var writer = new Utf8JsonWriter(buffer);

        writer.WriteStartObject();

        // Write original_keys directly — no intermediate List<string>/string[] allocation
        writer.WritePropertyName("original_keys"u8);
        if (data.ValueKind == JsonValueKind.Object)
        {
            writer.WriteStartArray();
            foreach (var prop in data.EnumerateObject())
                writer.WriteStringValue(prop.Name);
            writer.WriteEndArray();
        }
        else
        {
            writer.WriteNullValue();
        }

        // Write transformed_data in-place
        writer.WritePropertyName("transformed_data"u8);
        WriteTransformed(writer, data);

        writer.WriteString("server_type"u8, ServerType);

        writer.WriteEndObject();
        writer.Flush();

        return System.Text.Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    // ── Simulated DB query ──

    [McpServerTool(Name = "simulate_database_query"), Description("Simulates a database query with configurable delay")]
    public static async Task<string> SimulateDatabaseQuery(
        [Description("SQL query string")] string query,
        [Description("Delay in milliseconds (0-5000)")] int delay_ms = 0)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(delay_ms, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(delay_ms, 5000);

        if (delay_ms > 0)
            await Task.Delay(delay_ms).ConfigureAwait(false);

        return JsonSerializer.Serialize(
            new DbQueryResult(query, delay_ms, DateTime.UtcNow.ToString("O"), ServerType),
            BenchmarkJsonContext.Default.DbQueryResult);
    }

    // ── Recursive string transformer writing directly to Utf8JsonWriter ──

    private static void WriteTransformed(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var prop in element.EnumerateObject())
                {
                    writer.WritePropertyName(prop.Name);
                    WriteTransformed(writer, prop.Value);
                }
                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                    WriteTransformed(writer, item);
                writer.WriteEndArray();
                break;

            case JsonValueKind.String:
                writer.WriteStringValue(element.GetString()?.ToUpperInvariant());
                break;

            default:
                element.WriteTo(writer);
                break;
        }
    }
}

