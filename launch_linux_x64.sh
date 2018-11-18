git pull

dotnet publish --self-contained -r ubuntu.16.10-x64 -c Release

cd TheGodfather
dotnet ef database update

./bin/Release/netcoreapp2.1/ubuntu.16.10-x64/TheGodfather

