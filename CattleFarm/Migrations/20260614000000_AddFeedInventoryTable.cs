using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CattleFarm.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedInventoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_FeedInventories_FarmId_FeedType",
                table: "FeedInventories",
                columns: new[] { "FarmId", "FeedType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedInventories");
        }
    }
}
