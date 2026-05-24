using CattleFarm.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CattleFarm.Migrations
{
    [DbContext(typeof(CattleFarmDbContext))]
    [Migration("20260524021000_MakeTaskAssignedWorkerNullable")]
    public partial class MakeTaskAssignedWorkerNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_Workers_AssignedWorkerId",
                table: "TaskAssignments");

            migrationBuilder.AlterColumn<int>(
                name: "AssignedWorkerId",
                table: "TaskAssignments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_Workers_AssignedWorkerId",
                table: "TaskAssignments",
                column: "AssignedWorkerId",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_Workers_AssignedWorkerId",
                table: "TaskAssignments");

            migrationBuilder.AlterColumn<int>(
                name: "AssignedWorkerId",
                table: "TaskAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_Workers_AssignedWorkerId",
                table: "TaskAssignments",
                column: "AssignedWorkerId",
                principalTable: "Workers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
