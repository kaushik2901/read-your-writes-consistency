CREATE TABLE [dbo].[ProjectMembers]
(
    [ProjectId] INT NOT NULL,
    [UserId] INT NOT NULL,
    [AddedAtUtc] DATETIME2(3) NOT NULL CONSTRAINT [DF_ProjectMembers_AddedAtUtc] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_ProjectMembers] PRIMARY KEY ([ProjectId], [UserId]),
    CONSTRAINT [FK_ProjectMembers_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProjectMembers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
);


