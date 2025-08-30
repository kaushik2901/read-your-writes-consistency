CREATE PROCEDURE [dbo].[System_GetCurrentLSN]
AS
BEGIN
	SELECT log_end_lsn AS LSN FROM sys.dm_db_log_stats(DB_ID());
END
