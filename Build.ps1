function VerifySuccessExitCode {
    if ($LastExitCode -ne 0) {
        throw "Command failed with exit code $LastExitCode."
    }
}

Write-Host "$(pwsh --version)"
Write-Host "Active .NET SDK: $(dotnet --version)"

# In a PR the base branch needs to be fetched in order for regitlint to work.
function FetchBaseBranchIfNotMaster() {
    if ($env:APPVEYOR_PULL_REQUEST_NUMBER -And $env:APPVEYOR_REPO_BRANCH -ne "master") {
        git fetch -q origin ${env:APPVEYOR_REPO_BRANCH}:${env:APPVEYOR_REPO_BRANCH}
    }
}

FetchBaseBranchIfNotMaster

dotnet tool restore
VerifySuccessExitCode

dotnet build --configuration Release --version-suffix="pre"
VerifySuccessExitCode

dotnet test --no-build --configuration Release --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.DeterministicReport=true
VerifySuccessExitCode

dotnet reportgenerator -reports:**\coverage.cobertura.xml -targetdir:artifacts\coverage -filefilters:-*.g.cs
VerifySuccessExitCode

dotnet pack --no-build --configuration Release --output artifacts/packages --version-suffix="pre"
VerifySuccessExitCode
