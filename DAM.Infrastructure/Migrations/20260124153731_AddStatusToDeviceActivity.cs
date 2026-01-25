using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToDeviceActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DeviceActivities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "DeviceActivities");
        }
    }
}
