#!/bin/bash

if [ "$#" -e 1 ]; then
	echo "Waiting for the bot to shutdown... "
	wait "$1"
fi

echo "Downloading ... "
curl -LO "https://ci.appveyor.com/api/projects/ivan-ristovic/the-godfather/artifacts/TheGodfather.zip"
curl -LO "https://ci.appveyor.com/api/projects/ivan-ristovic/the-godfather/artifacts/TheGodfatherResources.zip"

echo "Extracting ... "
unzip -o TheGodfather.zip
unzip -o TheGodfatherResources.zip ./Resources/

echo "Starting the bot... "
dotnet TheGodfather.dll