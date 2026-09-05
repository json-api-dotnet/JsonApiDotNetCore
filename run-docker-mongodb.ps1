#Requires -Version 7.0
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

Push-Location $PSScriptRoot
try {
    docker compose --file docker-compose-mongodb.yml down
    docker compose --file docker-compose-mongodb.yml up --detach --wait
}
finally {
    Pop-Location
}
