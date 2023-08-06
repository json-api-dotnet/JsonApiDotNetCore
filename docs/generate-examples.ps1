#Requires -Version 7.0

# This script generates HTTP response files (*.json) for .ps1 files in ./request-examples

function Get-WebServer-ProcessId {
    $webProcessId = $null
    if ($IsMacOS -Or $IsLinux) {
        $webProcessId = $(lsof -ti:14141)
    }
    elseif ($IsWindows) {
        $webProcessId = $(Get-NetTCPConnection -LocalPort 14141 -ErrorAction SilentlyContinue).OwningProcess?[0]
    }
    else {
        throw "Unsupported operating system."
    }

    return $webProcessId
}

function Stop-WebServer {
    $webProcessId = Get-WebServer-ProcessId

    if ($webProcessId -ne $null) {
        Write-Output "Stopping web server"
        Get-Process -Id $webProcessId | Stop-Process -ErrorVariable stopErrorMessage

        if ($stopErrorMessage) {
            throw "Failed to stop web server: $stopErrorMessage"
        }
    }
}

function Start-WebServer {
    Write-Output "Starting web server"
    $startTimeUtc = Get-Date -AsUTC
    $job = Start-Job -ScriptBlock {
        dotnet run --project ..\src\Examples\GettingStarted\GettingStarted.csproj --configuration Release --property:TreatWarningsAsErrors=True --urls=http://0.0.0.0:14141
    }

    $webProcessId = $null
    $timeout = [timespan]::FromSeconds(30)

    Do {
        Start-Sleep -Seconds 1
        $hasTimedOut = ($(Get-Date -AsUTC) - $startTimeUtc) -gt $timeout
        $webProcessId = Get-WebServer-ProcessId
    } While ($webProcessId -eq $null -and -not $hasTimedOut)

    if ($hasTimedOut) {
        Write-Host "Failed to start web server, dumping output."
        Receive-Job -Job $job
        throw "Failed to start web server."
    }
}

Stop-WebServer
Start-WebServer

try {
    Remove-Item -Force -Path .\request-examples\*.json

    $scriptFiles = Get-ChildItem .\request-examples\*.ps1
    foreach ($scriptFile in $scriptFiles) {
        $jsonFileName = [System.IO.Path]::GetFileNameWithoutExtension($scriptFile.Name) + "_Response.json"

        Write-Output "Writing file: $jsonFileName"
        & $scriptFile.FullName > .\request-examples\$jsonFileName

        if ($LastExitCode -ne 0) {
            throw "Example request from '$($scriptFile.Name)' failed with exit code $LastExitCode."
        }
    }
}
finally {
    Stop-WebServer
}
