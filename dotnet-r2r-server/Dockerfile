FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /app

# Copy csproj and the referenced AOT source for restore and build
COPY dotnet-r2r-server/McpDotnetR2rServer.csproj ./dotnet-r2r-server/
COPY dotnet-aot-server/BenchmarkTools.cs ./dotnet-aot-server/
RUN dotnet restore dotnet-r2r-server/McpDotnetR2rServer.csproj

# Copy remaining R2R server source and publish with ReadyToRun
COPY dotnet-r2r-server/ ./dotnet-r2r-server/
RUN dotnet publish dotnet-r2r-server/McpDotnetR2rServer.csproj -c Release -o /out

# Final image — uses ASP.NET runtime (R2R still requires the runtime)
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=builder /out .

# Expose port
EXPOSE 8086

ENTRYPOINT ["dotnet", "McpDotnetR2rServer.dll"]

