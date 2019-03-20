#!/bin/bash

function execute {
	if ! "$@"; then
		echo "An error occured."
		exit 1
	fi
}


if [ "$#" -ne 0 ]; then
	echo "Waiting for the bot to shutdown... "
	wait "$1"
fi

rm TheGodfather*.zip &2> /dev/null

echo "Downloading ... "
execute wget "https://ci.appveyor.com/api/projects/ivan-ristovic/the-godfather/artifacts/TheGodfather.zip" -q --show-progress
execute wget "https://ci.appveyor.com/api/projects/ivan-ristovic/the-godfather/artifacts/TheGodfatherResources.zip" -q --show-progress

echo "Extracting ... "
execute unzip -o TheGodfather.zip
mkdir -p Resources
execute unzip -o TheGodfatherResources.zip -d Resources/

echo "Starting the bot... "
execute dotnet TheGodfather.dll
