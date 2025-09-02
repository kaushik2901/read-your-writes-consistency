CREATE PROCEDURE [dbo].[SeedData_Upsert]
AS
BEGIN
    SET NOCOUNT ON;

    -- Users
    IF NOT EXISTS (SELECT 1 FROM [dbo].[Users])
    BEGIN
        INSERT INTO [dbo].[Users] ([UserName], [DisplayName], [Email])
        VALUES
            (N'alice', N'Alice Johnson', N'alice@example.com'),
            (N'bob', N'Bob Smith', N'bob@example.com'),
            (N'carol', N'Carol Lee', N'carol@example.com');
    END

    DECLARE @AliceId INT = (SELECT [Id] FROM [dbo].[Users] WHERE [UserName] = N'alice');
    DECLARE @BobId INT = (SELECT [Id] FROM [dbo].[Users] WHERE [UserName] = N'bob');
    DECLARE @CarolId INT = (SELECT [Id] FROM [dbo].[Users] WHERE [UserName] = N'carol');

    -- Projects
    IF NOT EXISTS (SELECT 1 FROM [dbo].[Projects])
    BEGIN
        INSERT INTO [dbo].[Projects] ([Name], [CreatedByUserId])
        VALUES
            (N'Apollo', @AliceId),
            (N'Zephyr', @BobId),
            (N'Orion', @CarolId);
    END

    DECLARE @ApolloId INT = (SELECT [Id] FROM [dbo].[Projects] WHERE [Name] = N'Apollo');
    DECLARE @ZephyrId INT = (SELECT [Id] FROM [dbo].[Projects] WHERE [Name] = N'Zephyr');
    DECLARE @OrionId INT = (SELECT [Id] FROM [dbo].[Projects] WHERE [Name] = N'Orion');

    -- Project Members
    MERGE [dbo].[ProjectMembers] AS target
    USING (VALUES
        (@ApolloId, @AliceId),
        (@ApolloId, @BobId),
        (@ZephyrId, @BobId),
        (@ZephyrId, @CarolId),
        (@OrionId, @CarolId),
        (@OrionId, @AliceId)
    ) AS src([ProjectId], [UserId])
    ON (target.[ProjectId] = src.[ProjectId] AND target.[UserId] = src.[UserId])
    WHEN NOT MATCHED BY TARGET THEN
        INSERT ([ProjectId], [UserId]) VALUES (src.[ProjectId], src.[UserId]);

    -- Tasks
    IF NOT EXISTS (SELECT 1 FROM [dbo].[Tasks])
    BEGIN
        INSERT INTO [dbo].[Tasks] ([ProjectId], [Name], [Status], [AssignedUserId])
        VALUES
            (@ApolloId, N'Design API endpoints', N'New', @AliceId),
            (@ApolloId, N'Implement Dapper queries', N'Active', @BobId),
            (@ApolloId, N'Set up CI/CD', N'Blocked', @AliceId),
            (@ZephyrId, N'Create UI components', N'Active', @CarolId),
            (@ZephyrId, N'Write unit tests', N'New', @BobId),
            (@OrionId, N'Draft project charter', N'New', @CarolId);
    END

    -- Consistencies
    IF NOT EXISTS (SELECT 1 FROM [dbo].[Consistencies])
    BEGIN
        INSERT INTO [dbo].[Consistencies] ([Timestamp])
        VALUES
            (SYSUTCDATETIME());
    END
END


