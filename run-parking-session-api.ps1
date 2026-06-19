$ErrorActionPreference = "Stop"

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ParkingSession__UseInMemoryStore = "true"
$env:Api__UseHttpsRedirection = "false"

$apiPath = Join-Path $PSScriptRoot "src/PBMS.API"
$logPath = Join-Path $PSScriptRoot "pbms-api-run.log"

Set-Location $apiPath
& "C:\Program Files\dotnet\dotnet.exe" run --no-build --configuration Release --launch-profile "parking-session-test" *> $logPath
