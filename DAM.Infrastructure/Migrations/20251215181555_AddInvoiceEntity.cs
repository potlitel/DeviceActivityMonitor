using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SerialNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoice");
        }
    }
}
