# Gets the version suffix from the repo tag
# example: v1.0.0-preview1-final => preview1-final
function Get-Version-Suffix-From-Tag {
  $tag=$env:APPVEYOR_REPO_TAG_NAME
  $split=$tag -split "-"
  $suffix=$split[1..2]
  $final=$suffix -join "-"
  return $final
}

function CheckLastExitCode {
    param ([int[]]$SuccessCodes = @(0), [scriptblock]$CleanupScript=$null)

    if ($SuccessCodes -notcontains $LastExitCode) {
        $msg = "EXE RETURNED EXIT CODE $LastExitCode"
        throw $msg
    }
}

$revision = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = 1 }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$revision = "{0:D4}" -f [convert]::ToInt32($revision, 10)

dotnet restore
CheckLastExitCode

dotnet build -c Release
CheckLastExitCode

dotnet test -c Release --no-build
CheckLastExitCode

Write-Output "APPVEYOR_REPO_TAG: $env:APPVEYOR_REPO_TAG"

if ($env:APPVEYOR_REPO_TAG -eq $true) {
    $revision = Get-Version-Suffix-From-Tag
    Write-Output "VERSION-SUFFIX: $revision"

    if ([string]::IsNullOrWhitespace($revision)) {
        Write-Output "RUNNING dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts"
                              dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts
        CheckLastExitCode
    }
    else {
        Write-Output "RUNNING dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts --version-suffix=$revision"
                              dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts --version-suffix=$revision
        CheckLastExitCode
    }
}
else {
    $packageVersionSuffix="rc-$revision"
    Write-Output "VERSION-SUFFIX: $packageVersionSuffix"
    Write-Output "RUNNING dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts --version-suffix=$packageVersionSuffix"
                          dotnet pack .\src\JsonApiDotNetCore -c Release -o .\artifacts --version-suffix=$packageVersionSuffix
    CheckLastExitCode
}
