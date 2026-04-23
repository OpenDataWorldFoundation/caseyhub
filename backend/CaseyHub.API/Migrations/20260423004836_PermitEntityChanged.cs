using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaseyHub.API.Migrations
{
    /// <inheritdoc />
    public partial class PermitEntityChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StageDecision",
                table: "Permits",
                newName: "ServiceArea");

            migrationBuilder.AddColumn<DateTime>(
                name: "AdvertiseCommencedDate",
                table: "Permits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AdvertiseCompletedDate",
                table: "Permits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Decision",
                table: "Permits",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionStage",
                table: "Permits",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdvertiseCommencedDate",
                table: "Permits");

            migrationBuilder.DropColumn(
                name: "AdvertiseCompletedDate",
                table: "Permits");

            migrationBuilder.DropColumn(
                name: "Decision",
                table: "Permits");

            migrationBuilder.DropColumn(
                name: "DecisionStage",
                table: "Permits");

            migrationBuilder.RenameColumn(
                name: "ServiceArea",
                table: "Permits",
                newName: "StageDecision");
        }
    }
}
