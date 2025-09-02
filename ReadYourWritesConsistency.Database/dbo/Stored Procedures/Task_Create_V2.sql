CREATE PROCEDURE [dbo].[Task_Create_V2]
    @RequestingUserId INT,
    @ProjectId INT,
    @Name NVARCHAR(200),
    @AssignedUserId INT,
    @Status NVARCHAR(50) = N'New'
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM [dbo].[ProjectMembers]
        WHERE [ProjectId] = @ProjectId AND [UserId] = @RequestingUserId
    )
    BEGIN
        RAISERROR('User has no access to the project.', 16, 1);
        RETURN;
    END

    INSERT INTO [dbo].[Tasks]([ProjectId], [Name], [Status], [AssignedUserId])
    VALUES (@ProjectId, @Name, @Status, @AssignedUserId);

    UPDATE [dbo].[Projects]
    SET [LastUpdatedAtUtc] = SYSUTCDATETIME()
    WHERE [Id] = @ProjectId;

    EXEC [dbo].[Consistency_CaptureCommit];
END


