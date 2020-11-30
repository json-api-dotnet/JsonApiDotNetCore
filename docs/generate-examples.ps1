#Requires -Version 7.0

# This script generates response documents for ./request-examples

function Kill-WebServer {
    $processId = $null
    if ($IsMacOs || $IsLinux) {
        $processId = $(lsof -ti:8080)
    }
    elseif ($IsWindows) {
        $processId = $(Get-NetTCPConnection -LocalPort 14141 -ErrorAction SilentlyContinue).OwningProcess
    }

    if ($processId -ne $null) {
        Write-Output "Stopping web server"
        Get-Process -Id $processId | Stop-Process
    }

}

function Start-Webserver {
    Write-Output "Starting web server"
    Start-Job -ScriptBlock { dotnet run --project ..\src\Examples\GettingStarted\GettingStarted.csproj } | Out-Null
}

Kill-WebServer
Start-Webserver

Remove-Item -Force -Path .\request-examples\*.json

Start-Sleep -Seconds 10

$scriptFiles = Get-ChildItem .\request-examples\*.ps1
foreach ($scriptFile in $scriptFiles) {
    $jsonFileName = [System.IO.Path]::GetFileNameWithoutExtension($scriptFile.Name) + "_Response.json"

    Write-Output "Writing file: $jsonFileName"
    & $scriptFile.FullName > .\request-examples\$jsonFileName

    if ($LastExitCode -ne 0) {
        throw [System.Exception] "Example request from '$($scriptFile.Name)' failed."
    }
}

Kill-WebServer
