using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingInsurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniquenessOnRiskFactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "RiskFactorConfigurations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "BuildingType",
                table: "RiskFactorConfigurations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AdjustmentPercentage",
                table: "RiskFactorConfigurations",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateIndex(
                name: "IX_RiskFactorConfigurations_Level_BuildingType",
                table: "RiskFactorConfigurations",
                columns: new[] { "Level", "BuildingType" },
                unique: true,
                filter: "[BuildingType] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RiskFactorConfigurations_Level_ReferenceId",
                table: "RiskFactorConfigurations",
                columns: new[] { "Level", "ReferenceId" },
                unique: true,
                filter: "[ReferenceId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RiskFactorConfigurations_Level_BuildingType",
                table: "RiskFactorConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_RiskFactorConfigurations_Level_ReferenceId",
                table: "RiskFactorConfigurations");

            migrationBuilder.AlterColumn<int>(
                name: "Level",
                table: "RiskFactorConfigurations",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "BuildingType",
                table: "RiskFactorConfigurations",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AdjustmentPercentage",
                table: "RiskFactorConfigurations",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,4)",
                oldPrecision: 5,
                oldScale: 4);
        }
    }
}
