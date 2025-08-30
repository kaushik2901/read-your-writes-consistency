CREATE PROCEDURE [dbo].[Projects_GetDashboard_V1]
    @RequestingUserId INT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH ProjectStats AS (
        SELECT
            p.[Id],
            p.[Name],
            MAX(p.[LastUpdatedAtUtc]) AS [LastUpdatedAtUtc],
            SUM(CASE WHEN t.[Status] = N'New' THEN 1 ELSE 0 END) AS [NewCount],
            SUM(CASE WHEN t.[Status] = N'Active' THEN 1 ELSE 0 END) AS [ActiveCount],
            SUM(CASE WHEN t.[Status] = N'Blocked' THEN 1 ELSE 0 END) AS [BlockedCount]
        FROM [dbo].[Projects] p
        INNER JOIN [dbo].[ProjectMembers] pm ON pm.[ProjectId] = p.[Id]
        LEFT JOIN [dbo].[Tasks] t ON t.[ProjectId] = p.[Id]
        WHERE pm.[UserId] = @RequestingUserId
        GROUP BY p.[Id], p.[Name]
    )
    SELECT
        [Id], [Name], [NewCount], [ActiveCount], [BlockedCount], [LastUpdatedAtUtc]
    FROM ProjectStats
    ORDER BY [Name];
END


