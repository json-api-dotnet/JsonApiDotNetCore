#Requires -Version 7

function ShutdownCosmosDbEmulator {
    if ($PSVersionTable.Platform -eq "Unix") {
        ShutdownCosmosDbEmulatorDockerContainer
    }
    else {
        ShutdownCosmosDbEmulatorForWindows
    }
}

function ShutdownCosmosDbEmulatorDockerContainer {
    Write-Host "Shutting down Cosmos DB Emulator Docker container ..."
    docker stop azure-cosmos-emulator-linux
}

function ShutdownCosmosDbEmulatorForWindows {
    Write-Host "Shutting down Cosmos DB Emulator for Windows ..."
    Start-Process -FilePath "C:\Program Files\Azure Cosmos DB Emulator\Microsoft.Azure.Cosmos.Emulator.exe" -ArgumentList "/Shutdown"
}

ShutdownCosmosDbEmulator
