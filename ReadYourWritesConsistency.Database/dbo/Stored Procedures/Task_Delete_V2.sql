CREATE PROCEDURE [dbo].[Task_Delete_V2]
    @RequestingUserId INT,
    @TaskId INT
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

    DELETE FROM [dbo].[Tasks]
    WHERE [Id] = @TaskId;

    UPDATE [dbo].[Projects]
    SET [LastUpdatedAtUtc] = SYSUTCDATETIME()
    WHERE [Id] = @ProjectId;

    EXEC [dbo].[Consistency_CaptureCommit];
END


