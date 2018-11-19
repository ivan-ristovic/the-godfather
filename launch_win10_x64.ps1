git pull

dotnet publish --self-contained -r win-x64 -c Release 
.\Dependencies\rcedit-x64.exe "TheGodfather\bin\Release\netcoreapp2.1\win-x64\TheGodfather.exe" --set-icon "TheGodfather\icon.ico"

cd .\TheGodfather
dotnet ef database update

cd .\bin\Release\netcoreapp2.1\win-x64
Start-Process -FilePath ".\TheGodfather.exe"
