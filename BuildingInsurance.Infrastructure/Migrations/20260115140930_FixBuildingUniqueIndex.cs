using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingInsurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBuildingUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Buildings_ClientId",
                table: "Buildings");

           
            migrationBuilder.CreateIndex(
                name: "IX_Buildings_ClientId_CityId_AddressStreet_AddressNumber",
                table: "Buildings",
                columns: new[] { "ClientId", "CityId", "AddressStreet", "AddressNumber" },
                unique: true,
                filter: "[AddressStreet] IS NOT NULL AND [AddressNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Buildings_ClientId_CityId_AddressStreet_AddressNumber",
                table: "Buildings");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_ClientId",
                table: "Buildings",
                column: "ClientId");
        }
    }
}
