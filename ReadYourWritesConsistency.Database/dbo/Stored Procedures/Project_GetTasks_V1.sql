CREATE PROCEDURE [dbo].[Project_GetTasks_V1]
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

    SELECT
        t.[Id], t.[Name], t.[Status], u.[Id] AS [AssignedUserId], u.[DisplayName] AS [UserName], t.[LastModifiedAtUtc]
    FROM [dbo].[Tasks] t
    LEFT JOIN [dbo].[Users] u ON u.[Id] = t.[AssignedUserId]
    WHERE t.[ProjectId] = @ProjectId
    ORDER BY t.[Id] DESC;
END


