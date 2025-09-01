#!/bin/bash
set -e

echo "=== SQL Server Transactional Replication Deployment ==="

# Wait for primary
echo "Waiting for primary SQL Server..."
until /opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -Q "SELECT 1" -C &> /dev/null; do
  >&2 echo "Primary not ready..."
  sleep 3
done
echo "✓ Primary is up."

# Wait for replica
echo "Waiting for replica SQL Server..."
until /opt/mssql-tools/bin/sqlcmd -S db-replica -U sa -P "$SA_PASSWORD" -Q "SELECT 1" -C &> /dev/null; do
  >&2 echo "Replica not ready..."
  sleep 3
done
echo "✓ Replica is up."

# Additional wait to ensure SQL Agent is ready
echo "Waiting for SQL Server Agent to be ready..."
sleep 10

echo "=== DEPLOYING DATABASE SCHEMA ==="

echo "Deploying dacpac to primary..."
sqlpackage /a:Publish \
  /SourceFile:"ReadYourWritesConsistency.Database.dacpac" \
  /TargetServerName:db \
  /TargetDatabaseName:ReadYourWritesConsistency \
  /TargetUser:sa \
  /TargetPassword:"$SA_PASSWORD" \
  /TargetTrustServerCertificate:True \
  /p:BlockOnPossibleDataLoss=false

echo "✓ Primary database deployed."

echo "Deploying dacpac to replica..."
sqlpackage /a:Publish \
  /SourceFile:"ReadYourWritesConsistency.Database.dacpac" \
  /TargetServerName:db-replica \
  /TargetDatabaseName:ReadYourWritesConsistency \
  /TargetUser:sa \
  /TargetPassword:"$SA_PASSWORD" \
  /TargetTrustServerCertificate:True \
  /p:BlockOnPossibleDataLoss=false

echo "✓ Replica database deployed."

echo "=== CONFIGURING TRANSACTIONAL REPLICATION ==="

# Configure replication on primary
echo "Setting up distributor and publisher on primary..."
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -C -i ./scripts/01_configure_distributor_publisher.sql
sleep 10
echo "✓ Distributor configured."

echo "Enabling database for replication..."
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d ReadYourWritesConsistency -C -i ./scripts/02_enable_database_for_replication.sql
sleep 10
echo "✓ Database enabled for replication."

echo "Creating publication..."
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d ReadYourWritesConsistency -C -i ./scripts/03_create_publication.sql
sleep 10
echo "✓ Publication created."

echo "Adding tables to publication..."
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d ReadYourWritesConsistency -C -i ./scripts/04_add_tables.sql
sleep 10
echo "✓ Articles added to publication."

echo "Adding subscriber..."
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d ReadYourWritesConsistency -C -i ./scripts/05_add_subscriber.sql
sleep 10
echo "✓ Subscription created."

echo "Starting snapshot agent..."
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d ReadYourWritesConsistency -C -i ./scripts/06_start_snapshot_agent.sql
sleep 60
echo "✓ Snapshot agent started."

echo "=== SEEDING INITIAL DATA ==="

echo "Seeding data in primary..."
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d ReadYourWritesConsistency -Q "EXEC dbo.SeedData_Upsert" -C

echo "✓ Primary data seeded."
echo ""
echo "=== DEPLOYMENT COMPLETED ==="