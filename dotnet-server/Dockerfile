FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

RUN apt-get update && apt-get install -y clang zlib1g-dev && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy csproj and restore first for layer caching
COPY McpDotnetServer.csproj .
RUN dotnet restore

# Copy all source and publish with AOT
COPY . .
RUN dotnet publish -c Release -o /out

# Final minimal image
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0

WORKDIR /app

COPY --from=builder /out .

# Expose port
EXPOSE 8084

# Run the AOT-compiled binary
ENTRYPOINT ["./McpDotnetServer"]

