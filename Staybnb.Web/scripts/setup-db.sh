#!/usr/bin/env bash

set -e

PASSWORD="${MSSQL_SA_PASSWORD:-YourStrong!Passw0rd}"
SERVER="${MSSQL_SERVER:-localhost,1433}"

echo "======================================"
echo " Staybnb database setup"
echo "======================================"
echo ""

echo "Checking Docker..."
if ! command -v docker >/dev/null 2>&1; then
    echo "ERROR: Docker is not installed or not available."
    exit 1
fi

echo "Checking sqlcmd..."
if ! command -v sqlcmd >/dev/null 2>&1; then
    echo "ERROR: sqlcmd is required."
    echo "Install SQL Server command-line tools, then run this script again."
    exit 1
fi

echo ""
echo "Checking SQL Server at ${SERVER}..."

for i in {1..60}; do
    if sqlcmd \
        -S "$SERVER" \
        -U sa \
        -P "$PASSWORD" \
        -C \
        -Q "SELECT 1" >/dev/null 2>&1; then

        echo "SQL Server is ready."
        break
    fi

    if [ "$i" -eq 60 ]; then
        echo "ERROR: SQL Server was not available."
        echo ""
        echo "Make sure Docker Desktop is running and SQL Server"
        echo "is available on localhost:1433."
        exit 1
    fi

    sleep 2
done

echo ""
echo "Checking StaybnbDb..."

if ! sqlcmd \
    -S "$SERVER" \
    -U sa \
    -P "$PASSWORD" \
    -C \
    -Q "IF DB_ID('StaybnbDb') IS NULL THROW 50000, 'StaybnbDb does not exist.', 1;" \
    >/dev/null 2>&1; then

    echo "StaybnbDb does not exist yet."
    echo "EF Core migrations will create it."
else
    echo "StaybnbDb found."
fi

echo ""
echo "Restoring .NET tools..."
dotnet tool restore

echo ""
echo "Applying EF Core migrations..."
dotnet ef database update

echo ""
echo "======================================"
echo " Database setup complete"
echo "======================================"
echo ""
echo "Start Staybnb with:"
echo "  dotnet run"
echo ""
