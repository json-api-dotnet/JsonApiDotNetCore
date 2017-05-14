$revision = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = 1 }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$revision = "{0:D4}" -f [convert]::ToInt32($revision, 10)

dotnet restore

dotnet test ./test/UnitTests/UnitTests.csproj
dotnet test ./test/JsonApiDotNetCoreExampleTests/JsonApiDotNetCoreExampleTests.csproj
dotnet test ./test/NoEntityFrameworkTests/NoEntityFrameworkTests.csproj

dotnet build .\src\JsonApiDotNetCore -c Release

echo "APPVEYOR_REPO_TAG: $env:APPVEYOR_REPO_TAG"
echo "VERSION-SUFFIX: alpha1-$revision"

If($env:APPVEYOR_REPO_TAG -eq $true) {
    echo "RUNNING dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts "
    dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts 
}
Else { 
    echo "RUNNING dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts --version-suffix=alpha1-$revision"
    dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts --version-suffix=alpha1-$revision 
}
