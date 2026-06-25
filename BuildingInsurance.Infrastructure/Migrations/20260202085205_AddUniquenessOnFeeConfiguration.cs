using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingInsurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniquenessOnFeeConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FeeConfigurations_FeeType_IsActive_EffectiveFrom_EffectiveTo",
                table: "FeeConfigurations");

            migrationBuilder.AlterColumn<string>(
                name: "RiskIndicators",
                table: "FeeConfigurations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_FeeConfigurations_FeeType_RiskIndicators_EffectiveFrom_EffectiveTo",
                table: "FeeConfigurations",
                columns: new[] { "FeeType", "RiskIndicators", "EffectiveFrom", "EffectiveTo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FeeConfigurations_FeeType_RiskIndicators_EffectiveFrom_EffectiveTo",
                table: "FeeConfigurations");

            migrationBuilder.AlterColumn<string>(
                name: "RiskIndicators",
                table: "FeeConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_FeeConfigurations_FeeType_IsActive_EffectiveFrom_EffectiveTo",
                table: "FeeConfigurations",
                columns: new[] { "FeeType", "IsActive", "EffectiveFrom", "EffectiveTo" });
        }
    }
}
