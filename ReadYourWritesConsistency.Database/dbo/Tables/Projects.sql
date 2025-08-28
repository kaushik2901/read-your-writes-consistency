CREATE TABLE [dbo].[Projects]
(
    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Projects] PRIMARY KEY,
    [Name] NVARCHAR(200) NOT NULL,
    [CreatedByUserId] INT NOT NULL,
    [LastUpdatedAtUtc] DATETIME2(3) NOT NULL CONSTRAINT [DF_Projects_LastUpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [FK_Projects_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[Users]([Id])
);


