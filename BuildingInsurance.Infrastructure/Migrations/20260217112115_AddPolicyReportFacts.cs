using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingInsurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyReportFacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PolicyReportFacts",
                columns: table => new
                {
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PolicyStatus = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinalPremium = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    FinalPremiumInBaseCurrency = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    BrokerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrokerCode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuildingType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceLastUpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyReportFacts", x => x.PolicyId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyReportFacts_BrokerCode_CurrencyId_PolicyStatus_StartDate",
                table: "PolicyReportFacts",
                columns: new[] { "BrokerCode", "CurrencyId", "PolicyStatus", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyReportFacts_CityId_CurrencyId_PolicyStatus_StartDate",
                table: "PolicyReportFacts",
                columns: new[] { "CityId", "CurrencyId", "PolicyStatus", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyReportFacts_CurrencyId_PolicyStatus_StartDate",
                table: "PolicyReportFacts",
                columns: new[] { "CurrencyId", "PolicyStatus", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyReportFacts_SourceLastUpdatedUtc",
                table: "PolicyReportFacts",
                column: "SourceLastUpdatedUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PolicyReportFacts");
        }
    }
}
