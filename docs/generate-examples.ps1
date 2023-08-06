#Requires -Version 7.0

# This script generates response documents for ./request-examples

function Get-WebServer-ProcessId {
    $processId = $null
    if ($IsMacOS -Or $IsLinux) {
        $processId = $(lsof -ti:14141)
    }
    elseif ($IsWindows) {
        $processId = $(Get-NetTCPConnection -LocalPort 14141 -ErrorAction SilentlyContinue).OwningProcess?[0]
    }
    else {
        throw [System.Exception] "Unsupported operating system."
    }

    Write-Host "Returning PID $processId"
    return $processId
}

function Kill-WebServer {
    $processId = Get-WebServer-ProcessId

    if ($processId -ne $null) {
        Write-Output "Stopping web server"
        Get-Process -Id $processId | Stop-Process -ErrorVariable stopErrorMessage

        if ($stopErrorMessage) {
            throw "Failed to stop web server: $stopErrorMessage"
        }
    }
}

function Start-WebServer {
    Write-Output "Starting web server"
    Start-Job -Name StartWebServer -ScriptBlock { dotnet run --project ..\src\Examples\GettingStarted\GettingStarted.csproj } | Out-Null
    Start-Sleep -Seconds 5
    Receive-Job -Name StartWebServer

    $webProcessId = $null
    Do {
        Start-Sleep -Seconds 1
        $webProcessId = Get-WebServer-ProcessId
    } While ($webProcessId -eq $null)
}

Kill-WebServer
Start-WebServer

try {
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
}
finally {
    Kill-WebServer
}
