CREATE PROCEDURE [dbo].[Consistency_CaptureCommit]
AS
BEGIN
    DECLARE @Id INT = 1;
    DECLARE @Timestamp DATETIME2(7) = SYSUTCDATETIME();

	IF NOT EXISTS (SELECT 1 FROM [dbo].[Consistencies])
    BEGIN
        INSERT INTO [dbo].[Consistencies] ([Id], [Timestamp])
        VALUES (@Id, @Timestamp);
    END
    ELSE
    BEGIN
        UPDATE [dbo].[Consistencies] SET [Timestamp] = @Timestamp WHERE Id = @Id;
    END

    SELECT @Timestamp AS Timestamp;
END
