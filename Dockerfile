# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /src

# Copy solution file
COPY AtermisShop/AtermisShop.sln ./

# Copy all project files (for better Docker layer caching)
COPY AtermisShop/AtermisShop_API/AtermisShop_API.csproj AtermisShop/AtermisShop_API/
COPY AtermisShop/AtermisShop.Domain/AtermisShop.Domain.csproj AtermisShop/AtermisShop.Domain/
COPY AtermisShop/AtermisShop.Application/AtermisShop.Application.csproj AtermisShop/AtermisShop.Application/
COPY AtermisShop/AtermisShop.Infrastructure/AtermisShop.Infrastructure.csproj AtermisShop/AtermisShop.Infrastructure/

# Restore dependencies (this leverages Docker cache)
RUN dotnet restore AtermisShop.sln

# Copy the rest of the source code
COPY AtermisShop/ AtermisShop/

# Build and publish the application
WORKDIR /src/AtermisShop
RUN dotnet publish AtermisShop_API/AtermisShop_API.csproj -c Release -o /app/publish --no-restore

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create a non-root user to run the application
RUN groupadd -r appuser && useradd -r -g appuser -s /bin/bash appuser

# Set the working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Copy entrypoint script (from root of build context)
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Change ownership of the application files
RUN chown -R appuser:appuser /app

# Switch to the non-root user
USER appuser

# Expose port (fly.io will set PORT env variable)
EXPOSE 8080

# Configure environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0
ENV DOTNET_gcServer=1

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:${PORT:-8080}/api/health || exit 1

# Run the application using entrypoint script
ENTRYPOINT ["/app/entrypoint.sh"]
