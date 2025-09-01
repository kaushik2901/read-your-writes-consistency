USE ReadYourWritesConsistency;
GO

EXEC sp_startpublication_snapshot 
    @publication = 'ReadYourWritesConsistency_Pub';
GO
