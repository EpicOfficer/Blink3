#!/bin/sh

set -e

echo "Replacing configuration values.."

# Replace placeholders in appsettings.json with corresponding environment variables
sed -i "s|\${API_ADDRESS}|${API_ADDRESS}|" /usr/share/nginx/html/appsettings.json
sed -i "s|\${CLIENT_ID}|${CLIENT_ID}|" /usr/share/nginx/html/appsettings.json

echo "Configuration values replaced!"