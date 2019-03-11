#!/bin/bash

if [ "$#" -ne 0 ]; then
	echo "Waiting for the bot to shutdown... "
	wait "$1"
fi

rm TheGodfather*.zip

echo "Downloading ... "
wget "https://ci.appveyor.com/api/projects/ivan-ristovic/the-godfather/artifacts/TheGodfather.zip" -q --show-progress
wget "https://ci.appveyor.com/api/projects/ivan-ristovic/the-godfather/artifacts/TheGodfatherResources.zip" -q --show-progress

echo "Extracting ... "
unzip -o TheGodfather.zip
unzip -o TheGodfatherResources.zip ./Resources/

echo "Starting the bot... "
dotnet TheGodfather.dll
