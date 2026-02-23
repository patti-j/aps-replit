#!/bin/bash
set -e
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://0.0.0.0:5000"

cd /home/runner/workspace/aps-web/WebAPI

pkill -9 OmniSharp 2>/dev/null || true
sleep 1

echo "Restoring packages..."
dotnet restore --force

echo "Building..."
dotnet build --no-restore

echo "Starting application..."
dotnet run --no-build --no-launch-profile
