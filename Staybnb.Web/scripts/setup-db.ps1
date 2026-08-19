$ErrorActionPreference = "Stop"

Write-Host "Starting SQL Server..."
docker compose up -d sqlserver

$password = if ($env:MSSQL_SA_PASSWORD) {
    $env:MSSQL_SA_PASSWORD
} else {
    "YourStrong!Passw0rd"
}

Write-Host "Waiting for SQL Server to become available..."

for ($i = 1; $i -le 60; $i++) {
    $result = docker exec staybnb-sqlserver /opt/mssql-tools18/bin/sqlcmd `
        -S localhost `
        -U sa `
        -P "$password" `
        -C `
        -Q "SELECT 1" 2>$null

    if ($LASTEXITCODE -eq 0) {
        Write-Host "SQL Server is ready."
        break
    }

    if ($i -eq 60) {
        Write-Error "SQL Server did not become ready in time."
        docker logs staybnb-sqlserver
        exit 1
    }

    Start-Sleep -Seconds 2
}

Write-Host "Applying EF Core migrations..."
dotnet ef database update

Write-Host ""
Write-Host "Database setup complete."
Write-Host "Run the application with:"
Write-Host "  dotnet run"
