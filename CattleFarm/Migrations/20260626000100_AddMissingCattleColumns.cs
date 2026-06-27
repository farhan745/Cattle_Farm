using CattleFarm.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CattleFarm.Migrations
{
    [DbContext(typeof(CattleFarmDbContext))]
    [Migration("20260626000100_AddMissingCattleColumns")]
    public partial class AddMissingCattleColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.Cattles', 'Category') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[Cattles]
                    ADD [Category] int NOT NULL CONSTRAINT [DF_Cattles_Category] DEFAULT 0;
                END

                IF COL_LENGTH('dbo.Cattles', 'Origin') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[Cattles]
                    ADD [Origin] nvarchar(200) NULL;
                END

                IF COL_LENGTH('dbo.Cattles', 'TransferredTo') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[Cattles]
                    ADD [TransferredTo] nvarchar(200) NULL;
                END

                IF COL_LENGTH('dbo.Cattles', 'TransferDate') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[Cattles]
                    ADD [TransferDate] datetime2 NULL;
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.Cattles', 'TransferDate') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[Cattles] DROP COLUMN [TransferDate];
                END

                IF COL_LENGTH('dbo.Cattles', 'TransferredTo') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[Cattles] DROP COLUMN [TransferredTo];
                END

                IF COL_LENGTH('dbo.Cattles', 'Origin') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[Cattles] DROP COLUMN [Origin];
                END

                IF COL_LENGTH('dbo.Cattles', 'Category') IS NOT NULL
                BEGIN
                    DECLARE @defaultConstraintName sysname;

                    SELECT @defaultConstraintName = dc.name
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c
                        ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Cattles')
                        AND c.name = N'Category';

                    IF @defaultConstraintName IS NOT NULL
                    BEGIN
                        EXEC(N'ALTER TABLE [dbo].[Cattles] DROP CONSTRAINT [' + @defaultConstraintName + N']');
                    END

                    ALTER TABLE [dbo].[Cattles] DROP COLUMN [Category];
                END
                """);
        }
    }
}
