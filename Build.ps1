function VerifySuccessExitCode {
    if ($LastExitCode -ne 0) {
        throw "Command failed with exit code $LastExitCode."
    }
}

Write-Host "$(pwsh --version)"
Write-Host ".NET SDK $(dotnet --version)"

Remove-Item -Recurse -Force artifacts -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force * -Include coverage.cobertura.xml

dotnet tool restore
VerifySuccessExitCode

dotnet build --configuration Release
VerifySuccessExitCode

dotnet test --no-build --configuration Release --verbosity quiet --collect:"XPlat Code Coverage"
VerifySuccessExitCode

dotnet reportgenerator -reports:**\coverage.cobertura.xml -targetdir:artifacts\coverage -filefilters:-*.g.cs
VerifySuccessExitCode

dotnet pack --no-build --configuration Release --output artifacts/packages
VerifySuccessExitCode
