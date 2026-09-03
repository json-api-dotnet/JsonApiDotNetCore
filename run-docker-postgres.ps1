#Requires -Version 7.0
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

Push-Location $PSScriptRoot
try {
    docker compose down
    docker compose up --detach --wait
}
finally {
    Pop-Location
}
