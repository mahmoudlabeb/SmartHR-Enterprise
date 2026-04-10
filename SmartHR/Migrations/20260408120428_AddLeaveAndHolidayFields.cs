using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHR.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveAndHolidayFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Leaves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnnualLeaveBalance",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_ApprovedByEmployeeId",
                table: "Leaves",
                column: "ApprovedByEmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Leaves_ApprovedByEmployeeId",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "AnnualLeaveBalance",
                table: "Employees");
        }
    }
}
