using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaseyHub.API.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalAttrToLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HouseNumber",
                table: "Permits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Municipality",
                table: "Permits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Permits",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HouseNumber",
                table: "Permits");

            migrationBuilder.DropColumn(
                name: "Municipality",
                table: "Permits");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Permits");
        }
    }
}
