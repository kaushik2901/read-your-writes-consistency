USE ReadYourWritesConsistency;
GO

-- Add subscriber
EXEC sp_addsubscriber 
    @subscriber = 'db-replica',
    @type = 0,
    @login = 'sa',
    @password = 'Password123$',
    @security_mode = 0;
GO

-- Create subscription
EXEC sp_addsubscription 
    @publication = 'ReadYourWritesConsistency_Pub',
    @subscriber = 'db-replica',
    @destination_db = 'ReadYourWritesConsistency',
    @subscription_type = 'Push',
    @sync_type = 'automatic',
    @article = 'all',
    @update_mode = 'read only';
GO

-- Add distribution agent
EXEC sp_addpushsubscription_agent 
    @publication = 'ReadYourWritesConsistency_Pub',
    @subscriber = 'db-replica',
    @subscriber_db = 'ReadYourWritesConsistency',
    @subscriber_security_mode = 0,
    @subscriber_login = 'sa',
    @subscriber_password = 'Password123$',
    @frequency_type = 64,
    @frequency_subday = 4,
    @frequency_subday_interval = 5;
GO