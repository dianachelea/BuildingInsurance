using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingInsurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyReportFactTableAndProcessingCheckpoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessingCheckpoint",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastRunUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingCheckpoint", x => x.Name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingCheckpoint_LastRunUtc",
                table: "ProcessingCheckpoint",
                column: "LastRunUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessingCheckpoint");
        }
    }
}
