using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CattleFarm.Migrations
{
    /// <inheritdoc />
    public partial class AppointmentVetWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletionNotes",
                table: "Appointments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvidenceImagePath",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrescriptionPath",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true);

            // Remap legacy Scheduled/Completed/Cancelled/NoShow to new enum values
            migrationBuilder.Sql(@"
UPDATE Appointments SET Status = CASE Status
    WHEN 0 THEN 1
    WHEN 1 THEN 2
    WHEN 2 THEN 3
    WHEN 3 THEN 5
    ELSE Status END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE Appointments SET Status = CASE Status
    WHEN 0 THEN 0
    WHEN 1 THEN 0
    WHEN 2 THEN 1
    WHEN 3 THEN 2
    WHEN 4 THEN 2
    WHEN 5 THEN 3
    ELSE Status END");

            migrationBuilder.DropColumn(name: "AcceptedAt", table: "Appointments");
            migrationBuilder.DropColumn(name: "CompletionNotes", table: "Appointments");
            migrationBuilder.DropColumn(name: "EvidenceImagePath", table: "Appointments");
            migrationBuilder.DropColumn(name: "PrescriptionPath", table: "Appointments");
        }
    }
}
