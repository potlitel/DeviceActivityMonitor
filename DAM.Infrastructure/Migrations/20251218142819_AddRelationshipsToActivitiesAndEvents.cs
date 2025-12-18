using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationshipsToActivitiesAndEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeviceActivityId",
                table: "Invoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeviceActivityId",
                table: "DevicePresences",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_DeviceActivityId",
                table: "Invoices",
                column: "DeviceActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_DevicePresences_DeviceActivityId",
                table: "DevicePresences",
                column: "DeviceActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_DevicePresences_DeviceActivities_DeviceActivityId",
                table: "DevicePresences",
                column: "DeviceActivityId",
                principalTable: "DeviceActivities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_DeviceActivities_DeviceActivityId",
                table: "Invoices",
                column: "DeviceActivityId",
                principalTable: "DeviceActivities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DevicePresences_DeviceActivities_DeviceActivityId",
                table: "DevicePresences");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_DeviceActivities_DeviceActivityId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_DeviceActivityId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_DevicePresences_DeviceActivityId",
                table: "DevicePresences");

            migrationBuilder.DropColumn(
                name: "DeviceActivityId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DeviceActivityId",
                table: "DevicePresences");
        }
    }
}
