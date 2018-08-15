dotnet publish --self-contained -r win10-x64 -c Release

.\Dependencies\rcedit-x64.exe "TheGodfather\bin\Release\netcoreapp2.1\win10-x64\TheGodfather.exe" --set-icon "TheGodfather\icon.ico"

Start-Process -FilePath ".\TheGodfather\bin\Release\netcoreapp2.1\win10-x64\TheGodfather.exe"
