using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeInsertedToPersistOnDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeInserted",
                table: "DeviceActivities",
                type: "time(7)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeInserted",
                table: "DeviceActivities");
        }
    }
}
