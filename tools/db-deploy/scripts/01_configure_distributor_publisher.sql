USE master;

EXEC sp_dropdistributor 1;
GO

-- Configure distributor
EXEC sp_adddistributor 
    @distributor = 'db',
    @password = 'Password123$';
GO

-- Configure distribution database
EXEC sp_adddistributiondb 
    @database = 'distribution',
    @data_folder = '/var/opt/mssql/data',
    @log_folder = '/var/opt/mssql/data',
    @security_mode = 1;
GO

USE distribution;
GO

DROP MASTER KEY;
GO

CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'Password123$';
GO

USE master;
GO

-- Configure publisher
EXEC sp_adddistpublisher 
    @publisher = 'db',
    @distribution_db = 'distribution',
    @security_mode = 1,
    @working_directory = '/var/opt/mssql/data';
GO