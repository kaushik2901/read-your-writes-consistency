CREATE PROCEDURE [dbo].[Users_GetAll]
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

