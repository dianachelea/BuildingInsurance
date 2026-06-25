using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingInsurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyAppliedRiskFactors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PolicyAppliedRiskFactors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RiskFactorConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BuildingType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdjustmentPercentage = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    AppliedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyAppliedRiskFactors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyAppliedRiskFactors_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PolicyAppliedRiskFactors_RiskFactorConfigurations_RiskFactorConfigurationId",
                        column: x => x.RiskFactorConfigurationId,
                        principalTable: "RiskFactorConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAppliedRiskFactors_PolicyId",
                table: "PolicyAppliedRiskFactors",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAppliedRiskFactors_PolicyId_RiskFactorConfigurationId",
                table: "PolicyAppliedRiskFactors",
                columns: new[] { "PolicyId", "RiskFactorConfigurationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAppliedRiskFactors_RiskFactorConfigurationId",
                table: "PolicyAppliedRiskFactors",
                column: "RiskFactorConfigurationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PolicyAppliedRiskFactors");
        }
    }
}
