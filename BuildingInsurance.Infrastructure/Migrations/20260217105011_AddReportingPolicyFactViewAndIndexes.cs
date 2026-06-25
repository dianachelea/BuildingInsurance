using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingInsurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportingPolicyFactViewAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Policies_CurrencyId",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Policies_StartDate_EndDate",
                table: "Policies");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Buildings",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_CurrencyId_PolicyStatus_StartDate",
                table: "Policies",
                columns: new[] { "CurrencyId", "PolicyStatus", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_Type_CityId",
                table: "Buildings",
                columns: new[] { "Type", "CityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Policies_CurrencyId_PolicyStatus_StartDate",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_Type_CityId",
                table: "Buildings");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Buildings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_CurrencyId",
                table: "Policies",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_StartDate_EndDate",
                table: "Policies",
                columns: new[] { "StartDate", "EndDate" });
        }
    }
}
