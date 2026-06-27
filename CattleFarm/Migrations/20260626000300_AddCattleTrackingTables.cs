using CattleFarm.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CattleFarm.Migrations
{
    [DbContext(typeof(CattleFarmDbContext))]
    [Migration("20260626000300_AddCattleTrackingTables")]
    public partial class AddCattleTrackingTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[WeightRecords]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[WeightRecords] (
                        [Id] int NOT NULL IDENTITY,
                        [MeasuredAt] datetime2 NOT NULL,
                        [WeightKg] decimal(8,2) NOT NULL,
                        [BodyConditionScore] nvarchar(200) NULL,
                        [Notes] nvarchar(500) NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [CattleId] int NOT NULL,
                        [FarmId] int NOT NULL,
                        [RecordedByUserId] int NULL,
                        CONSTRAINT [PK_WeightRecords] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_WeightRecords_Cattles_CattleId] FOREIGN KEY ([CattleId]) REFERENCES [dbo].[Cattles] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_WeightRecords_Farms_FarmId] FOREIGN KEY ([FarmId]) REFERENCES [dbo].[Farms] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_WeightRecords_Users_RecordedByUserId] FOREIGN KEY ([RecordedByUserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE SET NULL
                    );

                    CREATE INDEX [IX_WeightRecords_CattleId_MeasuredAt] ON [dbo].[WeightRecords] ([CattleId], [MeasuredAt]);
                    CREATE INDEX [IX_WeightRecords_FarmId] ON [dbo].[WeightRecords] ([FarmId]);
                    CREATE INDEX [IX_WeightRecords_RecordedByUserId] ON [dbo].[WeightRecords] ([RecordedByUserId]);
                END

                IF OBJECT_ID(N'[dbo].[HeatRecords]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[HeatRecords] (
                        [Id] int NOT NULL IDENTITY,
                        [ObservationDate] datetime2 NOT NULL,
                        [HeatStatus] int NOT NULL,
                        [HeatDurationHours] float NULL,
                        [NextExpectedHeatDate] datetime2 NULL,
                        [ObservedBy] nvarchar(200) NULL,
                        [DetectionMethod] nvarchar(20) NULL,
                        [ReadyForBreeding] bit NOT NULL,
                        [Notes] nvarchar(1000) NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [CattleId] int NOT NULL,
                        [FarmId] int NOT NULL,
                        CONSTRAINT [PK_HeatRecords] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_HeatRecords_Cattles_CattleId] FOREIGN KEY ([CattleId]) REFERENCES [dbo].[Cattles] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_HeatRecords_Farms_FarmId] FOREIGN KEY ([FarmId]) REFERENCES [dbo].[Farms] ([Id]) ON DELETE NO ACTION
                    );

                    CREATE INDEX [IX_HeatRecords_CattleId_ObservationDate] ON [dbo].[HeatRecords] ([CattleId], [ObservationDate]);
                    CREATE INDEX [IX_HeatRecords_FarmId] ON [dbo].[HeatRecords] ([FarmId]);
                END

                IF OBJECT_ID(N'[dbo].[BullPerformanceRecords]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[BullPerformanceRecords] (
                        [Id] int NOT NULL IDENTITY,
                        [EvaluationDate] datetime2 NOT NULL,
                        [MotilityPercent] decimal(5,2) NULL,
                        [MorphologyPercent] decimal(5,2) NULL,
                        [ConcentrationMillionPerMl] decimal(10,2) NULL,
                        [VolumeML] decimal(5,2) NULL,
                        [QualityGrade] int NOT NULL,
                        [EvaluatedBy] nvarchar(100) NULL,
                        [DosesCollected] int NULL,
                        [Cost] decimal(10,2) NULL,
                        [Notes] nvarchar(1000) NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [CattleId] int NOT NULL,
                        [FarmId] int NOT NULL,
                        CONSTRAINT [PK_BullPerformanceRecords] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_BullPerformanceRecords_Cattles_CattleId] FOREIGN KEY ([CattleId]) REFERENCES [dbo].[Cattles] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_BullPerformanceRecords_Farms_FarmId] FOREIGN KEY ([FarmId]) REFERENCES [dbo].[Farms] ([Id]) ON DELETE NO ACTION
                    );

                    CREATE INDEX [IX_BullPerformanceRecords_CattleId_EvaluationDate] ON [dbo].[BullPerformanceRecords] ([CattleId], [EvaluationDate]);
                    CREATE INDEX [IX_BullPerformanceRecords_FarmId] ON [dbo].[BullPerformanceRecords] ([FarmId]);
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[BullPerformanceRecords]', N'U') IS NOT NULL
                    DROP TABLE [dbo].[BullPerformanceRecords];

                IF OBJECT_ID(N'[dbo].[HeatRecords]', N'U') IS NOT NULL
                    DROP TABLE [dbo].[HeatRecords];

                IF OBJECT_ID(N'[dbo].[WeightRecords]', N'U') IS NOT NULL
                    DROP TABLE [dbo].[WeightRecords];
                """);
        }
    }
}
