USE ReadYourWritesConsistency;
GO

DECLARE @table_name NVARCHAR(255);
DECLARE @sql NVARCHAR(MAX);
DECLARE table_cursor CURSOR FOR
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME NOT LIKE 'MS%'
  AND TABLE_NAME NOT LIKE 'sys%'
  AND TABLE_NAME NOT IN ('sysdiagrams','dtproperties');

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @table_name;

WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY
        EXEC sp_addarticle 
            @publication = 'ReadYourWritesConsistency_Pub',
            @article = @table_name,
            @source_owner = 'dbo',
            @source_object = @table_name,
            @type = 'logbased',
            @pre_creation_cmd = 'drop',
            @schema_option = 0x000000000803509F,
            @identityrangemanagementoption = 'manual',
            @destination_table = @table_name,
            @destination_owner = 'dbo';
        PRINT 'Added article: ' + @table_name;
    END TRY
    BEGIN CATCH
        PRINT 'Error adding article ' + @table_name + ': ' + ERROR_MESSAGE();
    END CATCH

    FETCH NEXT FROM table_cursor INTO @table_name;
END

CLOSE table_cursor;
DEALLOCATE table_cursor;