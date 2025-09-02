CREATE PROCEDURE [dbo].[Consistency_IsCaughtUp]
	@Timestamp DATETIME2(7)
AS
BEGIN
	DECLARE @Id INT = 1;
	DECLARE @Response BIT = 0;
	DECLARE @StoredTimestamp DATETIME2(7);
	
	SELECT @StoredTimestamp = [Timestamp] FROM [dbo].[Consistencies] WHERE Id = @Id;
	
	IF (@StoredTimestamp >= @Timestamp)
	BEGIN 
		SET @Response = 1;
	END

	SELECT @Response AS IsCaughtUp;
END
