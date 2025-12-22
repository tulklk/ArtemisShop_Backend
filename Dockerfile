# =========================
# Stage 1: Build & Publish
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1) Copy solution file
COPY ArtemisShop/ArtemisShop.sln ArtemisShop/

# 2) Copy project files (for Docker layer caching)
COPY ArtemisShop/ArtemisShop_API/ArtemisShop_API.csproj ArtemisShop/ArtemisShop_API/
COPY ArtemisShop/ArtemisShop.Application/ArtemisShop.Application.csproj ArtemisShop/ArtemisShop.Application/
COPY ArtemisShop/ArtemisShop.Domain/ArtemisShop.Domain.csproj ArtemisShop/ArtemisShop.Domain/
COPY ArtemisShop/ArtemisShop.Infrastructure/ArtemisShop.Infrastructure.csproj ArtemisShop/ArtemisShop.Infrastructure/

# 3) Restore
WORKDIR /src/ArtemisShop
RUN dotnet restore ArtemisShop.sln

# 4) Copy the rest of the source code
WORKDIR /src
COPY ArtemisShop/ ArtemisShop/

# 5) Publish API
WORKDIR /src/ArtemisShop
RUN dotnet publish ArtemisShop_API/ArtemisShop_API.csproj -c Release -o /app/publish --no-restore


# =========================
# Stage 2: Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# (Optional) install curl for HEALTHCHECK
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Fly thường map internal_port = 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Healthcheck (đảm bảo bạn có endpoint /api/health)
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/api/health || exit 1

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ArtemisShop_API.dll"]
