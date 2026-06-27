using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CattleFarm.Migrations
{
    /// <inheritdoc />
    public partial class AddSmartMonitoringAndOfflineSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutomatedFeedingCommands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmId = table.Column<int>(type: "int", nullable: false),
                    CattleId = table.Column<int>(type: "int", nullable: true),
                    ControllerId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FeedName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    QuantityKg = table.Column<double>(type: "float", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomatedFeedingCommands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomatedFeedingCommands_Cattles_CattleId",
                        column: x => x.CattleId,
                        principalTable: "Cattles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AutomatedFeedingCommands_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeedInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmId = table.Column<int>(type: "int", nullable: false),
                    FeedType = table.Column<int>(type: "int", nullable: false),
                    StockQuantityKg = table.Column<double>(type: "float", nullable: false),
                    MinStockThresholdKg = table.Column<double>(type: "float", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedInventories_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GpsTrackerSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmId = table.Column<int>(type: "int", nullable: false),
                    CattleId = table.Column<int>(type: "int", nullable: true),
                    TrackerId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    SpeedKph = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GpsTrackerSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GpsTrackerSnapshots_Cattles_CattleId",
                        column: x => x.CattleId,
                        principalTable: "Cattles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GpsTrackerSnapshots_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MilkMachineImports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmId = table.Column<int>(type: "int", nullable: false),
                    CattleId = table.Column<int>(type: "int", nullable: true),
                    MachineId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    YieldLiters = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FatPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ProteinPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CollectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConvertedToMilkRecord = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilkMachineImports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MilkMachineImports_Cattles_CattleId",
                        column: x => x.CattleId,
                        principalTable: "Cattles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MilkMachineImports_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfflineSyncItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfflineSyncItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfflineSyncItems_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SensorReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmId = table.Column<int>(type: "int", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ReadingType = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BarnZone = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensorReadings_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomatedFeedingCommands_CattleId",
                table: "AutomatedFeedingCommands",
                column: "CattleId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomatedFeedingCommands_FarmId_ControllerId_ScheduledAt",
                table: "AutomatedFeedingCommands",
                columns: new[] { "FarmId", "ControllerId", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FeedInventories_FarmId_FeedType",
                table: "FeedInventories",
                columns: new[] { "FarmId", "FeedType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GpsTrackerSnapshots_CattleId",
                table: "GpsTrackerSnapshots",
                column: "CattleId");

            migrationBuilder.CreateIndex(
                name: "IX_GpsTrackerSnapshots_FarmId_TrackerId_RecordedAt",
                table: "GpsTrackerSnapshots",
                columns: new[] { "FarmId", "TrackerId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MilkMachineImports_CattleId",
                table: "MilkMachineImports",
                column: "CattleId");

            migrationBuilder.CreateIndex(
                name: "IX_MilkMachineImports_FarmId_MachineId_CollectedAt",
                table: "MilkMachineImports",
                columns: new[] { "FarmId", "MachineId", "CollectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OfflineSyncItems_FarmId_ClientId_Status",
                table: "OfflineSyncItems",
                columns: new[] { "FarmId", "ClientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_FarmId_DeviceId_RecordedAt",
                table: "SensorReadings",
                columns: new[] { "FarmId", "DeviceId", "RecordedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomatedFeedingCommands");

            migrationBuilder.DropTable(
                name: "FeedInventories");

            migrationBuilder.DropTable(
                name: "GpsTrackerSnapshots");

            migrationBuilder.DropTable(
                name: "MilkMachineImports");

            migrationBuilder.DropTable(
                name: "OfflineSyncItems");

            migrationBuilder.DropTable(
                name: "SensorReadings");
        }
    }
}
