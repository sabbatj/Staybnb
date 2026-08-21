#!/usr/bin/env bash

set -e

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

PASSWORD="${MSSQL_SA_PASSWORD:-YourStrong!Passw0rd}"

echo "======================================"
echo " Staybnb database setup"
echo "======================================"
echo ""

echo "Checking Docker..."

if ! command -v docker >/dev/null 2>&1; then
    echo "ERROR: Docker is not installed or not available."
    exit 1
fi

echo "Starting SQL Server..."

docker compose up -d sqlserver

echo ""
echo "Waiting for SQL Server to become available..."

for i in {1..60}; do
    if docker exec staybnb-sqlserver \
        /opt/mssql-tools18/bin/sqlcmd \
        -S localhost \
        -U sa \
        -P "$PASSWORD" \
        -C \
        -Q "SELECT 1" >/dev/null 2>&1; then

        echo "SQL Server is ready."
        break
    fi

    if [ "$i" -eq 60 ]; then
        echo "ERROR: SQL Server did not become ready in time."
        echo ""
        echo "Docker container logs:"
        docker logs staybnb-sqlserver
        exit 1
    fi

    sleep 2
done

echo ""
echo "Checking StaybnbDb..."

if docker exec staybnb-sqlserver \
    /opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "$PASSWORD" \
    -C \
    -Q "IF DB_ID('StaybnbDb') IS NULL THROW 50000, 'StaybnbDb does not exist.', 1;" \
    >/dev/null 2>&1; then

    echo "StaybnbDb found."

else

    echo "StaybnbDb does not exist yet."
    echo "EF Core migrations will create it."

fi

echo ""
echo "Restoring .NET tools..."

dotnet tool restore --tool-manifest "$ROOT_DIR/Staybnb.Web/.config/dotnet-tools.json"

echo ""
echo "Applying EF Core migrations..."

(cd "$ROOT_DIR/Staybnb.Web" && dotnet ef database update \
    --project "$ROOT_DIR/Staybnb.Web/Staybnb.Web.csproj" \
    --startup-project "$ROOT_DIR/Staybnb.Web/Staybnb.Web.csproj")

echo ""
echo "======================================"
echo " Database setup complete"
echo "======================================"
echo ""

echo "Start Staybnb with:"
echo "  dotnet run --project Staybnb.Web"
echo ""
