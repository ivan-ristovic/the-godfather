#!/bin/bash

echo "Waiting for the bot to shutdown... "
wait "$1"

echo "Downloading update... "
curl -LO "https://ci.appveyor.com/api/projects/ivan-ristovic/the-godfather/artifacts/TheGodfather.zip"
curl -LO "https://ci.appveyor.com/api/projects/ivan-ristovic/the-godfather/artifacts/TheGodfatherResources.zip"

echo "Extracting update... "
unzip -o TheGodfather.zip
unzip -o TheGodfatherResources.zip

echo "Starting the bot... "
dotnet TheGodfather.dll