# Gets the version suffix from the repo tag
# example: v1.0.0-preview1-final => preview1-final
function Get-Version-Suffix-From-Tag
{
  $tag=$env:APPVEYOR_REPO_TAG_NAME
  $split=$tag -split "-"
  $suffix=$split[1..2]
  $final=$suffix -join "-"
  return $final
}

function CheckLastExitCode {
    param ([string]$Command, [int[]]$SuccessCodes = @(0), [scriptblock]$CleanupScript=$null)

    if ($SuccessCodes -notcontains $LastExitCode) {
        throw "$Command exited with $LastExitCode"
    }
}

function Run($exp) {
    Invoke-Expression $exp
    CheckLastExitCode $exp
}

function BuildVerison($version) {
    Write-Output "Testing project against ASP.Net Core $version"
    $msBuildParams = "/p:TestProjectDependencyVersions=$version /p:NoWarn=NU1605 /v:minimal"

    Run "dotnet restore $msBuildParams"
    Run "dotnet test ./test/UnitTests/UnitTests.csproj $msBuildParams"
    Run "dotnet test ./test/JsonApiDotNetCoreExampleTests/JsonApiDotNetCoreExampleTests.csproj $msBuildParams"
    Run "dotnet test ./test/NoEntityFrameworkTests/NoEntityFrameworkTests.csproj $msBuildParams"
    Run "dotnet test ./test/OperationsExampleTests/OperationsExampleTests.csproj $msBuildParams"
}

$revision = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = 1 }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$revision = "{0:D4}" -f [convert]::ToInt32($revision, 10)

$supportedVersions = @("2.0.1", "2.1.0")
foreach ($version in $supportedVersions) {
	BuildVerison $version
}

Run "dotnet restore .\src\JsonApiDotNetCore"
Run "dotnet build .\src\JsonApiDotNetCore -c Release"

Write-Output "APPVEYOR_REPO_TAG: $env:APPVEYOR_REPO_TAG"

If($env:APPVEYOR_REPO_TAG -eq $true) {
    $revision = Get-Version-Suffix-From-Tag
    Write-Output "VERSION-SUFFIX: $revision"

    IF ([string]::IsNullOrWhitespace($revision)){
        Write-Output "RUNNING dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts"
        Run "dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts"        
    }
    Else {
        Write-Output "RUNNING dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts --version-suffix=$revision"
        Run "dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts --version-suffix=$revision"
    }
}
Else { 
    Write-Output "VERSION-SUFFIX: alpha1-$revision"
    Write-Output "RUNNING dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts --version-suffix=alpha1-$revision"
    Run "dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts --version-suffix=alpha1-$revision"
}
