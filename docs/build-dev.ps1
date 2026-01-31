#Requires -Version 7.4

# This script builds the documentation website, starts a web server and opens the site in your browser. Intended for local development.

param(
    # Specify -NoBuild to skip code build and examples generation. This runs faster, so handy when only editing Markdown files.
    [switch] $NoBuild=$false,
    # Specify -NoOpen to skip opening the documentation website in a web browser (still starts the web server).
    [switch] $NoOpen=$false
)

$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

# Workaround for bug at https://github.com/PowerShell/PowerShell/issues/23875#issuecomment-2672336383
$PSDefaultParameterValues['Remove-Item:ProgressAction'] = 'SilentlyContinue'

function EnsureDocfxBinaries() {
    # Temporary workaround until a proper DocFX build supporting .NET 10 is available.
    $zipFile = [IO.Path]::Combine($env:TEMP, 'docfx-net10-binaries.zip')

    if (!(Test-Path -Path 'docfx-net10-binaries')) {
        Invoke-WebRequest -Uri 'https://github.com/json-api-dotnet/docfx/raw/refs/heads/dotnet10-rtm/net10-binaries.zip' -Method 'GET' -OutFile $zipFile
        Expand-Archive $zipFile -Force
    }
}

function EnsureHttpServerIsInstalled {
    if ((Get-Command "npm" -ErrorAction SilentlyContinue) -eq $null) {
        throw "Unable to find npm in your PATH. please install Node.js first."
    }

    $global:hasHttpServerInstalled = $true

    & {
        $PSNativeCommandUseErrorActionPreference = $false

        # Workaround for error ENOENT returned from npm list, after installing Node.js on Windows.
        # See https://stackoverflow.com/questions/25093276/node-js-windows-error-enoent-stat-c-users-rt-appdata-roaming-npm
        if ($IsWindows) {
            New-Item -ItemType Directory -Force -Path $env:APPDATA\npm >$null
        }

        npm list --depth 1 --global httpserver >$null

        if ($LastExitCode -eq 1) {
            Write-Host "httpserver not found."
            $global:hasHttpServerInstalled = $false
        }
    }

    if ($global:hasHttpServerInstalled -eq $false) {
        Write-Host "Installing httpserver."
        npm install -g httpserver
    }
}

EnsureDocfxBinaries
EnsureHttpServerIsInstalled

if (-Not $NoBuild -Or -Not (Test-Path -Path _site)) {
    Remove-Item _site\* -Recurse -ErrorAction Ignore
    dotnet build .. --configuration Release /p:RunAnalyzers=false
    Invoke-Expression ./generate-examples.ps1
} else {
    Remove-Item _site\* -Recurse -ErrorAction Ignore
}

dotnet tool restore

$env:DOCFX_SOURCE_BRANCH_NAME="dev"
dotnet exec docfx-net10-binaries/docfx.dll docfx.json --warningsAsErrors true

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
Write-Host "Web server started. Site is available at http://localhost:8080. Press Enter to stop web server."
$key = [Console]::ReadKey()

Stop-Job -Id $webServerJob.Id
Get-job | Remove-Job -Force
