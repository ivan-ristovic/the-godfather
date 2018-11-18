git pull

dotnet publish --self-contained -r win10-x64 -c Release
.\Dependencies\rcedit-x64.exe "TheGodfather\bin\Release\netcoreapp2.1\win10-x64\TheGodfather.exe" --set-icon "TheGodfather\icon.ico"

cd .\TheGodfather
dotnet ef database update

cd .\bin\Release\netcoreapp2.1\win10-x64
Start-Process -FilePath ".\TheGodfather.exe"
