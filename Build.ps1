exec { & dotnet restore .\src\JsonApiDotNetCore\JsonApiDotNetCore.csproj }

$revision = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = 1 }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$revision = "{0:D4}" -f [convert]::ToInt32($revision, 10)

exec { & dotnet build .\src\JsonApiDotNetCore -c Release }

exec { & dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts --version-suffix=$revision }