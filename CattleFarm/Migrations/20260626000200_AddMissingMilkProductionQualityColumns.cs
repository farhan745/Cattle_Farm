using CattleFarm.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CattleFarm.Migrations
{
    [DbContext(typeof(CattleFarmDbContext))]
    [Migration("20260626000200_AddMissingMilkProductionQualityColumns")]
    public partial class AddMissingMilkProductionQualityColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.MilkProductions', 'FatPercentage') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[MilkProductions]
                    ADD [FatPercentage] decimal(5,2) NULL;
                END

                IF COL_LENGTH('dbo.MilkProductions', 'ProteinLevel') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[MilkProductions]
                    ADD [ProteinLevel] decimal(5,2) NULL;
                END

                IF COL_LENGTH('dbo.MilkProductions', 'SolidNotFat') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[MilkProductions]
                    ADD [SolidNotFat] decimal(5,2) NULL;
                END

                IF COL_LENGTH('dbo.MilkProductions', 'MilkQualityGrade') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[MilkProductions]
                    ADD [MilkQualityGrade] nvarchar(50) NULL;
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.MilkProductions', 'MilkQualityGrade') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[MilkProductions] DROP COLUMN [MilkQualityGrade];
                END

                IF COL_LENGTH('dbo.MilkProductions', 'SolidNotFat') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[MilkProductions] DROP COLUMN [SolidNotFat];
                END

                IF COL_LENGTH('dbo.MilkProductions', 'ProteinLevel') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[MilkProductions] DROP COLUMN [ProteinLevel];
                END

                IF COL_LENGTH('dbo.MilkProductions', 'FatPercentage') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[MilkProductions] DROP COLUMN [FatPercentage];
                END
                """);
        }
    }
}
