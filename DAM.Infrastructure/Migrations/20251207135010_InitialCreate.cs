using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SerialNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "TEXT", nullable: false),
                    TotalCapacityMB = table.Column<long>(type: "INTEGER", nullable: false),
                    InsertedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExtractedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    InitialAvailableMB = table.Column<long>(type: "INTEGER", nullable: false),
                    FinalAvailableMB = table.Column<long>(type: "INTEGER", nullable: false),
                    MegabytesCopied = table.Column<long>(type: "INTEGER", nullable: false),
                    MegabytesDeleted = table.Column<long>(type: "INTEGER", nullable: false),
                    FilesCopied = table.Column<string>(type: "TEXT", nullable: false),
                    FilesDeleted = table.Column<string>(type: "TEXT", nullable: false),
                    SpecialEvent = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceEvents", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceActivities");

            migrationBuilder.DropTable(
                name: "ServiceEvents");
        }
    }
}
