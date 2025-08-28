CREATE PROCEDURE [dbo].[Project_Delete]
    @RequestingUserId INT,
    @ProjectId INT
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

    DELETE FROM [dbo].[Projects]
    WHERE [Id] = @ProjectId;
END


