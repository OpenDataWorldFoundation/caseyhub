using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaseyHub.API.Migrations
{
    /// <inheritdoc />
    public partial class LocationAsAnObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Permits");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Permits",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Permits",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Postcode",
                table: "Permits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawAddress",
                table: "Permits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Permits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Suburb",
                table: "Permits",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Permits");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Permits");

            migrationBuilder.DropColumn(
                name: "Postcode",
                table: "Permits");

            migrationBuilder.DropColumn(
                name: "RawAddress",
                table: "Permits");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Permits");

            migrationBuilder.DropColumn(
                name: "Suburb",
                table: "Permits");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Permits",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
