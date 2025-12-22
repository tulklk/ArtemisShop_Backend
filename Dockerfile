# =========================
# Stage 1: Build & Publish
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy entire solution directory first (simpler approach)
COPY AtermisShop/ ./AtermisShop/

# Restore dependencies
WORKDIR /src/AtermisShop
RUN dotnet restore AtermisShop.sln

# Publish API project
RUN dotnet publish AtermisShop_API/AtermisShop_API.csproj -c Release -o /app/publish --no-restore

# =========================
# Stage 2: Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Configure ASP.NET Core
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/api/health || exit 1

# Copy published application
COPY --from=build /app/publish .

# Run the application
ENTRYPOINT ["dotnet", "AtermisShop_API.dll"]
