using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingInsurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Delete2Columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Buildings_ClientId_CityId_Building_AddressStreet_Building_AddressNumber",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "Building_AddressNumber",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "Building_AddressStreet",
                table: "Buildings");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_ClientId_CityId_AddressStreet_AddressNumber",
                table: "Buildings",
                columns: new[] { "ClientId", "CityId", "AddressStreet", "AddressNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Buildings_ClientId",
                table: "Buildings");

            migrationBuilder.AddColumn<string>(
                name: "Building_AddressNumber",
                table: "Buildings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Building_AddressStreet",
                table: "Buildings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_ClientId_CityId_Building_AddressStreet_Building_AddressNumber",
                table: "Buildings",
                columns: new[] { "ClientId", "CityId", "Building_AddressStreet", "Building_AddressNumber" },
                unique: true,
                filter: "[AddressStreet] IS NOT NULL AND [AddressNumber] IS NOT NULL");
        }
    }
}
