#!/bin/sh
set -e

# Get port from environment variable (set by fly.io) or use default 8080
PORT=${PORT:-8080}

# Set ASPNETCORE_URLS to listen on all interfaces with the specified port
export ASPNETCORE_URLS="http://0.0.0.0:${PORT}"

# Run the application
exec dotnet AtermisShop_API.dll
