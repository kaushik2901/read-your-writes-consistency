CREATE PROCEDURE [dbo].[Project_Get]
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

    SELECT p.[Id], p.[Name], p.[CreatedByUserId], p.[LastUpdatedAtUtc]
    FROM [dbo].[Projects] p
    WHERE p.[Id] = @ProjectId;

    SELECT u.[Id] AS [UserId], u.[DisplayName]
    FROM [dbo].[ProjectMembers] pm
    INNER JOIN [dbo].[Users] u ON u.[Id] = pm.[UserId]
    WHERE pm.[ProjectId] = @ProjectId
    ORDER BY u.[DisplayName];
END


