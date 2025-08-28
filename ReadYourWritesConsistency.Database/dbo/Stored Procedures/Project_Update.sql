CREATE PROCEDURE [dbo].[Project_Update]
    @RequestingUserId INT,
    @ProjectId INT,
    @Name NVARCHAR(200),
    @MemberUserIds [dbo].[UserIdList] READONLY
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

    UPDATE [dbo].[Projects]
    SET [Name] = COALESCE(@Name, [Name]),
        [LastUpdatedAtUtc] = SYSUTCDATETIME()
    WHERE [Id] = @ProjectId;

    IF EXISTS (SELECT 1 FROM @MemberUserIds)
    BEGIN
        -- Add new members
        INSERT INTO [dbo].[ProjectMembers]([ProjectId], [UserId])
        SELECT @ProjectId, m.[UserId]
        FROM @MemberUserIds m
        WHERE NOT EXISTS (
            SELECT 1 FROM [dbo].[ProjectMembers] pm
            WHERE pm.[ProjectId] = @ProjectId AND pm.[UserId] = m.[UserId]
        );

        -- Remove members not in the list, but never remove the creator
        DECLARE @CreatorId INT = (SELECT [CreatedByUserId] FROM [dbo].[Projects] WHERE [Id] = @ProjectId);

        DELETE pm
        FROM [dbo].[ProjectMembers] pm
        WHERE pm.[ProjectId] = @ProjectId
          AND pm.[UserId] <> @CreatorId
          AND NOT EXISTS (
              SELECT 1 FROM @MemberUserIds m
              WHERE m.[UserId] = pm.[UserId]
          );
    END
END


