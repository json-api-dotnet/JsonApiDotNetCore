#Requires -Version 7.0

# This script reformats the entire codebase to make it compliant with our coding guidelines.

#dotnet tool restore

dotnet tool uninstall --global jetbrains.resharper.globaltools
dotnet tool install --global jetbrains.resharper.globaltools --version 2022.2.0

dotnet tool uninstall --global regitlint
dotnet tool install --global regitlint --version 6.0.8

dotnet tool uninstall --global codecov.tool
dotnet tool install --global codecov.tool --version 1.13.0

dotnet tool uninstall --global dotnet-reportgenerator-globaltool
dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.1.3


dotnet restore

if ($LASTEXITCODE -ne 0) {
    throw "Package restore failed with exit code $LASTEXITCODE"
}

regitlint -s JsonApiDotNetCore.sln --print-command --disable-jb-path-hack --jb --profile='\"JADNC Full Cleanup\"' --jb --verbosity=WARN --use-global
