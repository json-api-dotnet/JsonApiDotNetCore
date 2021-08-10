#Requires -Version 7.0

# This script reformats the entire codebase to make it compliant with our coding guidelines.

dotnet tool restore

if ($LASTEXITCODE -ne 0) {
    throw "Tool restore failed with exit code $LASTEXITCODE"
}

dotnet build -c Release

if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE"
}

dotnet regitlint -s JsonApiDotNetCore.sln --print-command --jb --profile --jb --profile='\"JADNC Full Cleanup\"' --jb --properties:Configuration=Release --jb --verbosity=WARN
