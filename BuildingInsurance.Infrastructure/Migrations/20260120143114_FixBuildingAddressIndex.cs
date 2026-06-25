using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingInsurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBuildingAddressIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            IF COL_LENGTH('dbo.Buildings', 'Building_AddressStreet') IS NOT NULL
                ALTER TABLE dbo.Buildings DROP COLUMN [Building_AddressStreet];

            IF COL_LENGTH('dbo.Buildings', 'Building_AddressNumber') IS NOT NULL
                ALTER TABLE dbo.Buildings DROP COLUMN [Building_AddressNumber];
            ");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Building_AddressStreet",
                table: "Buildings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Building_AddressNumber",
                table: "Buildings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_ClientId_CityId_Building_AddressStreet_Building_AddressNumber",
                table: "Buildings",
                columns: new[] { "ClientId", "CityId", "Building_AddressStreet", "Building_AddressNumber" },
                unique: true,
                filter: "[Building_AddressStreet] IS NOT NULL AND [Building_AddressNumber] IS NOT NULL");
        }
    }
}
