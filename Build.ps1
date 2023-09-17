function VerifySuccessExitCode {
    if ($LastExitCode -ne 0) {
        throw "Command failed with exit code $LastExitCode."
    }
}

Write-Host "$(pwsh --version)"
Write-Host "Active .NET SDK: $(dotnet --version)"

dotnet tool restore
VerifySuccessExitCode

dotnet build --configuration Release --version-suffix="pre"
VerifySuccessExitCode

dotnet test --no-build --configuration Release --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.DeterministicReport=true
VerifySuccessExitCode

dotnet reportgenerator -reports:**\coverage.cobertura.xml -targetdir:artifacts\coverage
VerifySuccessExitCode

dotnet pack --no-build --configuration Release --output artifacts/packages --version-suffix="pre"
VerifySuccessExitCode
