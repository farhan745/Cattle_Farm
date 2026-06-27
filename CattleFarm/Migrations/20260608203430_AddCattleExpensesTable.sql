BEGIN TRANSACTION;
CREATE TABLE [CattleExpenses] (
    [Id] int NOT NULL IDENTITY,
    [Category] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Date] datetime2 NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsDeleted] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CattleId] int NOT NULL,
    [CreatedByUserId] int NULL,
    CONSTRAINT [PK_CattleExpenses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CattleExpenses_Cattles_CattleId] FOREIGN KEY ([CattleId]) REFERENCES [Cattles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CattleExpenses_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);

CREATE INDEX [IX_CattleExpenses_CattleId] ON [CattleExpenses] ([CattleId]);

CREATE INDEX [IX_CattleExpenses_CreatedByUserId] ON [CattleExpenses] ([CreatedByUserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260608203430_AddCattleExpensesTable', N'10.0.5');

COMMIT;
GO

