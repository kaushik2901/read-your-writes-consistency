CREATE PROCEDURE [dbo].[Task_Update_V1]
    @RequestingUserId INT,
    @TaskId INT,
    @Name NVARCHAR(200),
    @Status NVARCHAR(50),
    @AssignedUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ProjectId INT;
    SELECT @ProjectId = [ProjectId] FROM [dbo].[Tasks] WHERE [Id] = @TaskId;

    IF @ProjectId IS NULL
    BEGIN
        RAISERROR('Task not found.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (
        SELECT 1 FROM [dbo].[ProjectMembers]
        WHERE [ProjectId] = @ProjectId AND [UserId] = @RequestingUserId
    )
    BEGIN
        RAISERROR('User has no access to the project.', 16, 1);
        RETURN;
    END

    UPDATE [dbo].[Tasks]
    SET 
        [Name] = COALESCE(@Name, [Name]),
        [Status] = COALESCE(@Status, [Status]),
        [AssignedUserId] = COALESCE(@AssignedUserId, [AssignedUserId]),
        [LastModifiedAtUtc] = SYSUTCDATETIME()
    WHERE [Id] = @TaskId;

    UPDATE [dbo].[Projects]
    SET [LastUpdatedAtUtc] = SYSUTCDATETIME()
    WHERE [Id] = @ProjectId;
END


