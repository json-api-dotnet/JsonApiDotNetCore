#Requires -Version 7.4
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

Write-Host "$(pwsh --version)"
Write-Host ".NET SDK $(dotnet --version)"

Remove-Item -Recurse -Force artifacts -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force * -Include coverage.cobertura.xml

dotnet tool restore
dotnet build --configuration Release
dotnet test --no-build --configuration Release --verbosity quiet --collect:"XPlat Code Coverage"
dotnet reportgenerator -reports:**\coverage.cobertura.xml -targetdir:artifacts\coverage -filefilters:-*.g.cs
dotnet pack --no-build --configuration Release --output artifacts/packages
