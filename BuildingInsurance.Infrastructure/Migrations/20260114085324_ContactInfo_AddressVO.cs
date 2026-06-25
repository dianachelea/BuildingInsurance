using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingInsurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ContactInfo_AddressVO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Counties_CountyId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Counties_Countries_CountryId",
                table: "Counties");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Clients");

            migrationBuilder.AddColumn<string>(
                name: "AddressNumber",
                table: "Clients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AddressStreet",
                table: "Clients",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Counties_CountyId",
                table: "Cities",
                column: "CountyId",
                principalTable: "Counties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Counties_Countries_CountryId",
                table: "Counties",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Counties_CountyId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Counties_Countries_CountryId",
                table: "Counties");

            migrationBuilder.DropColumn(
                name: "AddressNumber",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "AddressStreet",
                table: "Clients");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Clients",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Counties_CountyId",
                table: "Cities",
                column: "CountyId",
                principalTable: "Counties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Counties_Countries_CountryId",
                table: "Counties",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
