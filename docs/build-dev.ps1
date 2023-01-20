#Requires -Version 7.0

# This script builds the documentation website, starts a web server and opens the site in your browser. Intended for local development.

param(
    # Specify -NoBuild to skip code build and examples generation. This runs faster, so handy when only editing Markdown files.
    [switch] $NoBuild=$False
)

function VerifySuccessExitCode {
    if ($LastExitCode -ne 0) {
        throw "Command failed with exit code $LastExitCode."
    }
}

function EnsureHttpServerIsInstalled {
    if ((Get-Command "npm" -ErrorAction SilentlyContinue) -eq $null) {
        throw "Unable to find npm in your PATH. please install Node.js first."
    }

    npm list --depth 1 --global httpserver >$null

    if ($LastExitCode -eq 1) {
        npm install -g httpserver
    }
}

EnsureHttpServerIsInstalled
VerifySuccessExitCode

if (-Not $NoBuild -Or -Not (Test-Path -Path _site)) {
    Remove-Item _site -Recurse -ErrorAction Ignore

    dotnet build .. --configuration Release
    VerifySuccessExitCode

    Invoke-Expression ./generate-examples.ps1
}

dotnet docfx ./docfx.json
VerifySuccessExitCode

Copy-Item -Force home/*.html _site/
Copy-Item -Force home/*.ico _site/
Copy-Item -Force -Recurse home/assets/* _site/styles/

cd _site
$webServerJob = httpserver &
Start-Process "http://localhost:8080/"
cd ..

Write-Host ""
Write-Host "Web server started. Press Enter to close."
$key = [Console]::ReadKey()

Stop-Job -Id $webServerJob.Id
Get-job | Remove-Job
