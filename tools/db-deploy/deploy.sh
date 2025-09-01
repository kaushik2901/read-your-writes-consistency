#!/bin/bash
set -e

echo "Waiting for SQL Server instances..."

# Wait for primary
until /opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -Q "SELECT 1" &> /dev/null; do
  >&2 echo "Primary not ready..."
  sleep 2
done
echo "Primary is up."

# Wait for replica
until /opt/mssql-tools/bin/sqlcmd -S db-replica -U sa -P "$SA_PASSWORD" -Q "SELECT 1" &> /dev/null; do
  >&2 echo "Replica not ready..."
  sleep 2
done
echo "Replica is up."

echo "Deploying dacpac to primary..."
sqlpackage /a:Publish \
  /SourceFile:"ReadYourWritesConsistency.Database.dacpac" \
  /TargetServerName:db \
  /TargetDatabaseName:ReadYourWritesConsistency \
  /TargetUser:sa \
  /TargetPassword:"$SA_PASSWORD" \
  /TargetTrustServerCertificate:True

echo "Seeding data in primary..."
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d ReadYourWritesConsistency -Q "EXEC dbo.SeedData_Upsert"

echo "Deploying dacpac to replica..."
sqlpackage /a:Publish \
  /SourceFile:"ReadYourWritesConsistency.Database.dacpac" \
  /TargetServerName:db-replica \
  /TargetDatabaseName:ReadYourWritesConsistency \
  /TargetUser:sa \
  /TargetPassword:"$SA_PASSWORD" \
  /TargetTrustServerCertificate:True

echo "Seeding data in replica..."
/opt/mssql-tools/bin/sqlcmd -S db-replica -U sa -P "$SA_PASSWORD" -d ReadYourWritesConsistency -Q "EXEC dbo.SeedData_Upsert"

echo "Configuring distributor on primary..."
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d master -Q "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'distribution') BEGIN EXEC sp_adddistributor @distributor = @@SERVERNAME, @password = N'$SA_PASSWORD' END"
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d master -Q "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'distribution') BEGIN EXEC sp_adddistributiondb @database = N'distribution', @data_folder = N'/var/opt/mssql/data', @log_folder = N'/var/opt/mssql/data', @log_file_size = 2, @min_distretention = 0, @max_distretention = 72, @history_retention = 48 END"
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d master -Q "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'distribution') BEGIN EXEC sp_adddistpublisher @publisher = @@SERVERNAME, @distribution_db = N'distribution', @security_mode = 0, @login = N'sa', @password = N'$SA_PASSWORD', @working_directory = N'/var/opt/mssql/ReplData/', @trusted = N'false', @thirdparty_flag = 0, @publisher_type = N'MSSQLSERVER' END"

echo "Enabling replication on primary DB..."
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -Q "EXEC sp_replicationdboption @dbname = N'ReadYourWritesConsistency', @optname = N'publish', @value = N'true';"

echo "Creating publication..."
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d ReadYourWritesConsistency -Q "EXEC sp_addpublication @publication = N'ReadYourWritesConsistency_Pub', @description = N'Transactional publication of ReadYourWritesConsistency database', @status = N'active', @allow_push = N'true', @independent_agent = N'true', @repl_freq = N'continuous';"
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d ReadYourWritesConsistency -Q "EXEC sp_addpublication_snapshot @publication = N'ReadYourWritesConsistency_Pub',@frequency_type = 1,@security_mode = 0,@login = N'sa',@password = N'$SA_PASSWORD';"

echo "Adding tables to publication..."
TABLES=$(/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d ReadYourWritesConsistency -h -1 -Q "SET NOCOUNT ON; SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = 'dbo';" | tr -d '\r')

for TABLE in $TABLES; do
  echo " -> $TABLE"
  /opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d ReadYourWritesConsistency -Q "IF NOT EXISTS (SELECT * FROM sysarticles a INNER JOIN syspublications b ON a.pubid = b.pubid WHERE a.name = '$TABLE' AND b.name = 'ReadYourWritesConsistency_Pub') BEGIN EXEC sp_addarticle @publication = N'ReadYourWritesConsistency_Pub', @article = N'$TABLE', @source_owner = N'dbo', @source_object = N'$TABLE', @type = N'logbased', @pre_creation_cmd = N'drop'; END"
done

echo "Creating push subscription..."
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d ReadYourWritesConsistency -Q "EXEC sp_addsubscription @publication = N'ReadYourWritesConsistency_Pub', @subscriber = N'db-replica', @destination_db = N'ReadYourWritesConsistency', @subscription_type = N'Push', @sync_type = N'automatic', @update_mode = N'read only', @subscriber_type = 0;"
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d ReadYourWritesConsistency -Q "EXEC sp_addpushsubscription_agent @publication = N'ReadYourWritesConsistency_Pub', @subscriber = N'db-replica', @subscriber_db = N'ReadYourWritesConsistency', @subscriber_security_mode = 0, @subscriber_login = N'sa', @subscriber_password = N'$SA_PASSWORD', @frequency_type = 64, @frequency_interval = 1, @frequency_subday = 4, @frequency_subday_interval = 5;"

echo "Starting replication agents..."
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d msdb -Q "DECLARE @snapshot_job NVARCHAR(128), @logreader_job NVARCHAR(128); SELECT @snapshot_job = name FROM sysjobs WHERE name LIKE '%ReadYourWritesConsistency_Pub%Snapshot%'; SELECT @logreader_job = name FROM sysjobs WHERE name LIKE '%ReadYourWritesConsistency_Pub%LogReader%'; IF @snapshot_job IS NOT NULL EXEC sp_start_job @job_name = @snapshot_job; IF @logreader_job IS NOT NULL EXEC sp_start_job @job_name = @logreader_job;"

echo "Checking replication status..."
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d distribution -Q "SELECT TOP 10 * FROM MSrepl_errors ORDER BY time DESC;"
/opt/mssql-tools/bin/sqlcmd -S db -U sa -P "$SA_PASSWORD" -d distribution -Q "EXEC sp_replmonitorhelpsubscription @publication = N'ReadYourWritesConsistency_Pub';"

echo "✅ Replication setup complete!"
