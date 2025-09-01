USE ReadYourWritesConsistency;
GO

-- Create publication
EXEC sp_addpublication 
    @publication = 'ReadYourWritesConsistency_Pub',
    @description = 'Transactional publication',
    @sync_method = 'concurrent',
    @retention = 60,
    @allow_push = 'true',
    @allow_pull = 'false',
    @snapshot_in_defaultfolder = 'true',
    @compress_snapshot = 'false',
    @repl_freq = 'continuous',
    @status = 'active',
    @independent_agent = 'true',
    @immediate_sync = 'true',
    @replicate_ddl = 1;
GO

-- Add snapshot agent
EXEC sp_addpublication_snapshot 
    @publication = 'ReadYourWritesConsistency_Pub',
    @frequency_type = 1,
    @publisher_security_mode = 1;
GO