using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CattleFarm.Migrations
{
    /// <inheritdoc />
    public partial class AddCattleMedicalRecordsAndGlobalDoctors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Farms_FarmId",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_FarmId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "FarmId",
                table: "Doctors");

            migrationBuilder.AddColumn<string>(
                name: "AvailableTimeSlot",
                table: "Doctors",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CattleMedicalRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CattleId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    ExaminationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChiefComplaint = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Diagnosis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Prescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MedicineName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MedicineDose = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DoseFrequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DoseDurationDays = table.Column<int>(type: "int", nullable: false),
                    NextVisitDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CattleMedicalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CattleMedicalRecords_Cattles_CattleId",
                        column: x => x.CattleId,
                        principalTable: "Cattles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CattleMedicalRecords_Users_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CattleMedicalRecords_CattleId",
                table: "CattleMedicalRecords",
                column: "CattleId");

            migrationBuilder.CreateIndex(
                name: "IX_CattleMedicalRecords_DoctorId",
                table: "CattleMedicalRecords",
                column: "DoctorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CattleMedicalRecords");

            migrationBuilder.DropColumn(
                name: "AvailableTimeSlot",
                table: "Doctors");

            migrationBuilder.AddColumn<int>(
                name: "FarmId",
                table: "Doctors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_FarmId",
                table: "Doctors",
                column: "FarmId");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Farms_FarmId",
                table: "Doctors",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
