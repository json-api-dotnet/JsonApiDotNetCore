#Requires -Version 7.0

# This script generates response documents for ./request-examples

function Get-WebServer-ProcessId {
    $processId = $null
    if ($IsMacOs || $IsLinux) {
        $processId = $(lsof -ti:14141)
    }
    elseif ($IsWindows) {
        $processId = $(Get-NetTCPConnection -LocalPort 14141 -ErrorAction SilentlyContinue).OwningProcess
    }
    else {
        throw [System.Exception] "Unsupported operating system."
    }

    return $processId
}

function Kill-WebServer {
    $processId = Get-WebServer-ProcessId

    if ($processId -ne $null) {
        Write-Output "Stopping web server"
        Get-Process -Id $processId | Stop-Process
    }
}

function Start-WebServer {
    Write-Output "Starting web server"
    Start-Job -ScriptBlock { dotnet run --project ..\src\Examples\GettingStarted\GettingStarted.csproj } | Out-Null

    $webProcessId = $null
    Do {
        Start-Sleep -Seconds 1
        $webProcessId = Get-WebServer-ProcessId
    } While ($webProcessId -eq $null)
}

Kill-WebServer
Start-WebServer

Remove-Item -Force -Path .\request-examples\*.json

$scriptFiles = Get-ChildItem .\request-examples\*.ps1
foreach ($scriptFile in $scriptFiles) {
    $jsonFileName = [System.IO.Path]::GetFileNameWithoutExtension($scriptFile.Name) + "_Response.json"

    Write-Output "Writing file: $jsonFileName"
    & $scriptFile.FullName > .\request-examples\$jsonFileName

    if ($LastExitCode -ne 0) {
        throw [System.Exception] "Example request from '$($scriptFile.Name)' failed with exit code $LastExitCode."
    }
}

Kill-WebServer
