#Requires -Version 7

function StartCosmosDbEmulator {
    if ($PSVersionTable.Platform -eq "Unix") {
        StartCosmosDbEmulatorDockerContainer
    }
    else {
        StartCosmosDbEmulatorForWindows
    }
}

function StartCosmosDbEmulatorDockerContainer {
    Write-Host "Starting Cosmos DB Emulator Docker container ..."
    Start-Process nohup 'bash ./run-docker-azure-cosmos-emulator-linux.sh'

    Write-Host "Waiting for Cosmos DB Emulator Docker container to start up ..."
    Start-Sleep -Seconds 30
    WaitUntilCosmosSqlApiEndpointIsReady

    Write-Host "Installing Cosmos DB Emulator certificates ..."
    Start-Sleep -Seconds 30
    Start-Process -FilePath ".\install-azure-cosmos-emulator-linux-certificates.sh"
}

function StartCosmosDbEmulatorForWindows {
    Write-Host "Starting Cosmos DB Emulator for Windows ..."
    Start-Process -FilePath "C:\Program Files\Azure Cosmos DB Emulator\Microsoft.Azure.Cosmos.Emulator.exe" -ArgumentList "/DisableRateLimiting /NoUI /NoExplorer"

    Write-Host "Waiting for Cosmos DB Emulator for Windows to start up ..."
    Start-Sleep -Seconds 10
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
