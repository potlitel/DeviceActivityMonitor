using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusIndexToDeviceActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DeviceActivity_Status",
                table: "DeviceActivities",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceActivity_Status",
                table: "DeviceActivities");
        }
    }
}
