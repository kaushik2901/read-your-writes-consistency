USE ReadYourWritesConsistency;
GO

EXEC sp_replicationdboption 
    @dbname = 'ReadYourWritesConsistency',
    @optname = 'publish',
    @value = 'true';
GO