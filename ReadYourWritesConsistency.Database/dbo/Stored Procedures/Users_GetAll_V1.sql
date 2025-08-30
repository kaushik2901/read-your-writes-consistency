CREATE PROCEDURE [dbo].[Users_GetAll_V1]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        [Id],
        [UserName],
        [DisplayName]
    FROM [dbo].[Users]
    ORDER BY [DisplayName];
END

