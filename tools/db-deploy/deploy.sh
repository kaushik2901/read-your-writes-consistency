#!/bin/bash
set -e

echo "Waiting for SQL Server to be available..."
# The db host is the service name in docker-compose
until /opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -Q "SELECT 1" &> /dev/null;
do
  >&2 echo "SQL Server is unavailable - sleeping"
  sleep 1
done
>&2 echo "SQL Server is up."

echo "Publishing dacpac..."
sqlpackage /a:Publish \
    /SourceFile:"ReadYourWritesConsistency.Database.dacpac" \
    /TargetServerName:db \
    /TargetDatabaseName:ReadYourWritesConsistency \
    /TargetUser:sa \
    /TargetPassword:"$SA_PASSWORD" \
    /TargetTrustServerCertificate:True

echo "Running seed data script..."
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d ReadYourWritesConsistency -Q "EXEC dbo.SeedData_Upsert"

echo "Database deployment and seeding completed."

