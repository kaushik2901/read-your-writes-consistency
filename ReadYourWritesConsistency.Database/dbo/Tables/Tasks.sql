CREATE TABLE [dbo].[Tasks]
(
    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Tasks] PRIMARY KEY,
    [ProjectId] INT NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Tasks_Status] DEFAULT (N'New'),
    [AssignedUserId] INT NULL,
    [CreatedAtUtc] DATETIME2(3) NOT NULL CONSTRAINT [DF_Tasks_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [LastModifiedAtUtc] DATETIME2(3) NOT NULL CONSTRAINT [DF_Tasks_LastModifiedAtUtc] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [CK_Tasks_Status] CHECK ([Status] IN (N'New', N'Active', N'Blocked')),
    CONSTRAINT [FK_Tasks_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Tasks_Users_AssignedUserId] FOREIGN KEY ([AssignedUserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE SET NULL
);


