# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["AtermisShop/AtermisShop.sln", "AtermisShop/"]
COPY ["AtermisShop/AtermisShop_API/AtermisShop_API.csproj", "AtermisShop/AtermisShop_API/"]
COPY ["AtermisShop/AtermisShop.Domain/AtermisShop.Domain.csproj", "AtermisShop/AtermisShop.Domain/"]
COPY ["AtermisShop/AtermisShop.Application/AtermisShop.Application.csproj", "AtermisShop/AtermisShop.Application/"]
COPY ["AtermisShop/AtermisShop.Infrastructure/AtermisShop.Infrastructure.csproj", "AtermisShop/AtermisShop.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "AtermisShop/AtermisShop.sln"

# Copy everything else and build
COPY AtermisShop/ AtermisShop/
WORKDIR /src/AtermisShop
RUN dotnet build "AtermisShop.sln" -c Release -o /app/build

# Publish the API project
FROM build AS publish
RUN dotnet publish "AtermisShop_API/AtermisShop_API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create a non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published app
COPY --from=publish /app/publish .

# Copy entrypoint script
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Change ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port (fly.io will set PORT env variable)
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:${PORT:-8080}/api/health || exit 1

# Run the app using entrypoint script
ENTRYPOINT ["/app/entrypoint.sh"]

