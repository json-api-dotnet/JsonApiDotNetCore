# This script assumes that you have already installed docfx and httpserver.
# If that's not the case, run the next commands:
#   choco install docfx -y
#   npm install -g httpserver

Remove-Item _site -Recurse -ErrorAction Ignore

dotnet build .. --configuration Release
Invoke-Expression ./generate-examples.ps1

docfx ./docfx.json
Copy-Item home/*.html _site/
Copy-Item home/*.ico _site/
Copy-Item -Recurse home/assets/* _site/styles/

cd _site
httpserver &
Start-Process "http://localhost:8080/"
cd ..

Write-Host ""
Write-Host "Web server started. Press Enter to close."
$key = [Console]::ReadKey()
