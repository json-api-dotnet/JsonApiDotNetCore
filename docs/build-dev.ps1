#Requires -Version 7.3

# This script builds the documentation website, starts a web server and opens the site in your browser. Intended for local development.

param(
    # Specify -NoBuild to skip code build and examples generation. This runs faster, so handy when only editing Markdown files.
    [switch] $NoBuild=$False,
    # Specify -NoOpen to skip opening the documentation website in a web browser.
    [switch] $NoOpen=$False
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

    # If this command fails with ENOENT after installing Node.js on Windows, manually create the directory %APPDATA%\npm.
    npm list --depth 1 --global httpserver >$null

    if ($LastExitCode -eq 1) {
        npm install -g httpserver
    }
}

EnsureHttpServerIsInstalled
VerifySuccessExitCode

if (-Not $NoBuild -Or -Not (Test-Path -Path _site)) {
    Remove-Item _site\* -Recurse -ErrorAction Ignore

    dotnet build .. --configuration Release
    VerifySuccessExitCode

    Invoke-Expression ./generate-examples.ps1
} else {
    Remove-Item _site\* -Recurse -ErrorAction Ignore
}

dotnet tool restore
VerifySuccessExitCode

$env:DOCFX_SOURCE_BRANCH_NAME="dev"
dotnet docfx ./docfx.json --warningsAsErrors true
VerifySuccessExitCode

Copy-Item -Force home/*.html _site/
Copy-Item -Force home/*.ico _site/
New-Item -Force _site/styles -ItemType Directory | Out-Null
Copy-Item -Force -Recurse home/assets/* _site/styles/

cd _site
$webServerJob = httpserver &
cd ..

if (-Not $NoOpen) {
    Start-Process "http://localhost:8080/"
}

Write-Host ""
Write-Host "Web server started. Press Enter to close."
$key = [Console]::ReadKey()

Stop-Job -Id $webServerJob.Id
Get-job | Remove-Job -Force
