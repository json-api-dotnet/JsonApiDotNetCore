#Requires -Version 7

function StartCosmosDbEmulator {
    if ($PSVersionTable.Platform -eq "Unix") {
        StartCosmosDbEmulatorOnLinux
    }
    else {
        StartCosmosDbEmulatorOnWindows
    }
}

function StartCosmosDbEmulatorOnLinux {
    Remove-Item .\nohup.*

    Write-Host "Running Azure Cosmos Emulator Docker container ..."
    Start-Process nohup './run-docker-azure-cosmos-emulator-linux.sh'
    Start-Sleep -Seconds 1

    Write-Host "Waiting 60 seconds before trying to download Azure Cosmos Emulator SSL certificate ..."
    Start-Sleep -Seconds 60

    Write-Host "--- BEGIN CONTENTS OF NOHUP.OUT ---"
    Get-Content .\nohup.out
    Write-Host "--- END CONTENTS OF NOHUP.OUT ---"

    Write-Host "Installing Azure Cosmos Emulator SSL certificate ..."
    Start-Process bash './install-azure-cosmos-emulator-linux-certificates.sh' -Wait
    Write-Host "Installed Azure Cosmos Emulator SSL certificate."
}

function StartCosmosDbEmulatorOnWindows {
    Write-Host "Starting Cosmos DB Emulator for Windows ..."
    Start-Process -FilePath "C:\Program Files\Azure Cosmos DB Emulator\Microsoft.Azure.Cosmos.Emulator.exe" -ArgumentList "/DisableRateLimiting /NoUI /NoExplorer"

    Write-Host "Waiting for Azure Cosmos Emulator for Windows to start up ..."
    Start-Sleep -Seconds 20
    WaitUntilCosmosSqlApiEndpointIsReady
}

function WaitUntilCosmosSqlApiEndpointIsReady {
    # See https://seddryck.wordpress.com/2020/01/05/running-automated-tests-with-the-azure-cosmos-db-emulator-on-appveyor/
    $attempt = 0; $max = 5

    do {
        $client = New-Object System.Net.Sockets.TcpClient([System.Net.Sockets.AddressFamily]::InterNetwork)

        try {
            $client.Connect("localhost", 8081)
            Write-Host "Cosmos SQL API endpoint listening. Connection successful."
        }
        catch {
            $client.Close()
            if($attempt -eq $max) {
                Write-Host "Cosmos SQL API endpoint is not listening. Aborting connection."
                throw "Cosmos SQL API endpoint is not listening. Aborting connection."
            } else {
                [int]$sleepTime = 5 * (++$attempt)
                Write-Host "Cosmos SQL API endpoint is not yet listening. Retry after $sleepTime seconds..."
                Start-Sleep -Seconds $sleepTime;
            }
        }
    } while(!$client.Connected -and $attempt -lt $max)
}

StartCosmosDbEmulator
