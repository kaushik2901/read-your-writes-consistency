CREATE PROCEDURE [dbo].[Project_Create_V1]
    @RequestingUserId INT,
    @Name NVARCHAR(200),
    @MemberUserIds [dbo].[UserIdList] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[Projects]([Name], [CreatedByUserId])
    VALUES (@Name, @RequestingUserId);

    DECLARE @ProjectId INT = SCOPE_IDENTITY();

    -- Ensure the creator is a member
    INSERT INTO [dbo].[ProjectMembers]([ProjectId], [UserId])
    VALUES (@ProjectId, @RequestingUserId);

    -- Add provided members (dedup with creator)
    INSERT INTO [dbo].[ProjectMembers]([ProjectId], [UserId])
    SELECT DISTINCT @ProjectId, m.[UserId]
    FROM @MemberUserIds m
    WHERE m.[UserId] <> @RequestingUserId
      AND NOT EXISTS (
          SELECT 1 FROM [dbo].[ProjectMembers] pm
          WHERE pm.[ProjectId] = @ProjectId AND pm.[UserId] = m.[UserId]
      );

    SELECT @ProjectId AS [ProjectId];
END


