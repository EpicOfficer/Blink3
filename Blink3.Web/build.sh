#!/bin/sh

set -e

echo "⬇️ Installing .NET 9..."
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 9.0 -InstallDir ./dotnet

DOTNET=./dotnet/dotnet

echo "⚙️ Restoring dependencies..."
$DOTNET restore Blink3.Web.csproj

echo "🔨 Publishing Blazor WASM..."
$DOTNET publish Blink3.Web.csproj -c Release -o output

echo "🔁 Injecting environment values into appsettings.json..."
APPSETTINGS="output/wwwroot/appsettings.json"

# Confirm the file exists first
if [ -f "$APPSETTINGS" ]; then
  sed -i '' "s|\${API_ADDRESS}|${API_ADDRESS}|" "$APPSETTINGS"
  sed -i '' "s|\${CLIENT_ID}|${CLIENT_ID}|" "$APPSETTINGS"
  echo "✅ appsettings.json updated!"
else
  echo "⚠️ appsettings.json not found at $APPSETTINGS"
  exit 1
fi

echo "✅ Build finished and ready for deployment"
