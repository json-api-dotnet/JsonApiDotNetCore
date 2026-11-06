#Requires -Version 7.4
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

Write-Host "$(pwsh --version)"
Write-Host ".NET SDK $(dotnet --version)"

Remove-Item -Recurse -Force artifacts -ErrorAction SilentlyContinue

dotnet tool restore
dotnet build --configuration Release
dotnet test --no-build --configuration Release --coverlet --results-directory artifacts/test-results
dotnet reportgenerator "-reports:artifacts/test-results/*cobertura*.xml" "-targetdir:artifacts/coverage/report" "-filefilters:-*.g.cs"
dotnet pack --no-build --configuration Release --output artifacts/packages
