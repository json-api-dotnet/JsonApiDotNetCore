$versionSuffix="pre"

function VerifySuccessExitCode {
    if ($LastExitCode -ne 0) {
        throw "Command failed with exit code $LastExitCode."
    }
}

Write-Host "$(pwsh --version)"
Write-Host "Active .NET SDK: $(dotnet --version)"
Write-Host "Using version suffix: $versionSuffix"

dotnet tool restore
VerifySuccessExitCode

dotnet build --configuration Release /p:VersionSuffix=$versionSuffix
VerifySuccessExitCode

dotnet test --no-build --configuration Release --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.DeterministicReport=true
VerifySuccessExitCode

dotnet reportgenerator -reports:**\coverage.cobertura.xml -targetdir:artifacts\coverage -filefilters:-*.g.cs
VerifySuccessExitCode

dotnet pack --no-build --configuration Release --output artifacts/packages /p:VersionSuffix=$versionSuffix
VerifySuccessExitCode
