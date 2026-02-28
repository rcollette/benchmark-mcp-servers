FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /app

# Copy csproj and the referenced AOT source for restore and build
COPY dotnet-jit-server/McpDotnetJitServer.csproj ./dotnet-jit-server/
COPY dotnet-aot-server/BenchmarkTools.cs ./dotnet-aot-server/
RUN dotnet restore dotnet-jit-server/McpDotnetJitServer.csproj

# Copy remaining JIT server source and publish
COPY dotnet-jit-server/ ./dotnet-jit-server/
RUN dotnet publish dotnet-jit-server/McpDotnetJitServer.csproj -c Release -o /out

# Final image — uses ASP.NET runtime (JIT)
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=builder /out .

# Expose port
EXPOSE 8085

ENTRYPOINT ["dotnet", "McpDotnetJitServer.dll"]


