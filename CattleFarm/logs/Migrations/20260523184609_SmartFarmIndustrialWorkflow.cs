using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CattleFarm.Migrations
{
    /// <inheritdoc />
    public partial class SmartFarmIndustrialWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FarmWorkers_FarmId",
                table: "FarmWorkers");

            migrationBuilder.DropIndex(
                name: "IX_FarmJoinRequests_FarmId",
                table: "FarmJoinRequests");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TaskAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FarmId",
                table: "TaskAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaximumCattle",
                table: "Farms",
                type: "int",
                nullable: false,
                defaultValue: 500);

            migrationBuilder.AddColumn<int>(
                name: "MaximumWorkers",
                table: "Farms",
                type: "int",
                nullable: false,
                defaultValue: 50);

            migrationBuilder.Sql("""
                UPDATE t
                SET FarmId = COALESCE(w.FarmId, ownerFarm.Id, firstFarm.Id, 0)
                FROM TaskAssignments t
                LEFT JOIN Workers w ON t.AssignedWorkerId = w.Id
                OUTER APPLY (
                    SELECT TOP 1 f.Id
                    FROM Farms f
                    WHERE f.OwnerId = t.CreatedBy AND f.IsDeleted = 0
                    ORDER BY f.Id
                ) ownerFarm
                OUTER APPLY (
                    SELECT TOP 1 f.Id
                    FROM Farms f
                    WHERE f.IsDeleted = 0
                    ORDER BY f.Id
                ) firstFarm
                WHERE t.FarmId = 0;
                """);

            migrationBuilder.CreateTable(
                name: "CattleComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CattleId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CattleComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CattleComments_Cattles_CattleId",
                        column: x => x.CattleId,
                        principalTable: "Cattles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CattleLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CattleId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CattleLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CattleLikes_Cattles_CattleId",
                        column: x => x.CattleId,
                        principalTable: "Cattles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CattleShares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CattleId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Channel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShareUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CattleShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CattleShares_Cattles_CattleId",
                        column: x => x.CattleId,
                        principalTable: "Cattles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmId = table.Column<int>(type: "int", nullable: false),
                    WorkerUserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartsAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OwnerNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Users_WorkerUserId",
                        column: x => x.WorkerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalaryHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmId = table.Column<int>(type: "int", nullable: false),
                    WorkerId = table.Column<int>(type: "int", nullable: false),
                    WorkerUserId = table.Column<int>(type: "int", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Bonus = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    TaskAssignmentId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryHistories_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryHistories_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_AssignedUserId",
                table: "TaskAssignments",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_FarmId",
                table: "TaskAssignments",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_Status",
                table: "TaskAssignments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FarmWorkers_FarmId_WorkerUserId",
                table: "FarmWorkers",
                columns: new[] { "FarmId", "WorkerUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_FarmJoinRequests_FarmId_WorkerUserId_Status",
                table: "FarmJoinRequests",
                columns: new[] { "FarmId", "WorkerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CattleComments_CattleId_UserId_CreatedAt",
                table: "CattleComments",
                columns: new[] { "CattleId", "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CattleLikes_CattleId_UserId",
                table: "CattleLikes",
                columns: new[] { "CattleId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CattleShares_CattleId",
                table: "CattleShares",
                column: "CattleId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_FarmId_WorkerUserId_Status",
                table: "LeaveRequests",
                columns: new[] { "FarmId", "WorkerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_WorkerUserId",
                table: "LeaveRequests",
                column: "WorkerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryHistories_FarmId",
                table: "SalaryHistories",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryHistories_WorkerId_Year_Month",
                table: "SalaryHistories",
                columns: new[] { "WorkerId", "Year", "Month" });

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_Farms_FarmId",
                table: "TaskAssignments",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_Farms_FarmId",
                table: "TaskAssignments");

            migrationBuilder.DropTable(
                name: "CattleComments");

            migrationBuilder.DropTable(
                name: "CattleLikes");

            migrationBuilder.DropTable(
                name: "CattleShares");

            migrationBuilder.DropTable(
                name: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "SalaryHistories");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_AssignedUserId",
                table: "TaskAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_FarmId",
                table: "TaskAssignments");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_Status",
                table: "TaskAssignments");

            migrationBuilder.DropIndex(
                name: "IX_FarmWorkers_FarmId_WorkerUserId",
                table: "FarmWorkers");

            migrationBuilder.DropIndex(
                name: "IX_FarmJoinRequests_FarmId_WorkerUserId_Status",
                table: "FarmJoinRequests");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TaskAssignments");

            migrationBuilder.DropColumn(
                name: "FarmId",
                table: "TaskAssignments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "MaximumCattle",
                table: "Farms");

            migrationBuilder.DropColumn(
                name: "MaximumWorkers",
                table: "Farms");

            migrationBuilder.CreateIndex(
                name: "IX_FarmWorkers_FarmId",
                table: "FarmWorkers",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmJoinRequests_FarmId",
                table: "FarmJoinRequests",
                column: "FarmId");
        }
    }
}
