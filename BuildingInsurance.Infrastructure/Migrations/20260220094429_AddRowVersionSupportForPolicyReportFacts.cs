using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingInsurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionSupportForPolicyReportFacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProcessingCheckpoint_LastRunUtc",
                table: "ProcessingCheckpoint");

            migrationBuilder.DropColumn(
                name: "LastProcessedPolicyId",
                table: "ProcessingCheckpoint");

            migrationBuilder.DropColumn(
                name: "LastRunUtc",
                table: "ProcessingCheckpoint");

            migrationBuilder.AddColumn<long>(
                name: "LastProcessedChangeVersion",
                table: "ProcessingCheckpoint",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ChangeVersion",
                table: "Policies",
                type: "bigint",
                nullable: false,
                computedColumnSql: "CONVERT(bigint, [RowVersion])",
                stored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeVersion",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "LastProcessedChangeVersion",
                table: "ProcessingCheckpoint");

            migrationBuilder.AddColumn<Guid>(
                name: "LastProcessedPolicyId",
                table: "ProcessingCheckpoint",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRunUtc",
                table: "ProcessingCheckpoint",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingCheckpoint_LastRunUtc",
                table: "ProcessingCheckpoint",
                column: "LastRunUtc");
        }
    }
}
