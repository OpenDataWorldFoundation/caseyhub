using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaseyHub.API.Migrations
{
    /// <inheritdoc />
    public partial class UserPermits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSavedPermits",
                columns: table => new
                {
                    ApplicationNumber = table.Column<string>(type: "character varying(100)", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSavedPermits", x => new { x.ApplicationNumber, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserSavedPermits_Permits_ApplicationNumber",
                        column: x => x.ApplicationNumber,
                        principalTable: "Permits",
                        principalColumn: "ApplicationNumber",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSavedPermits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSavedPermits_UserId",
                table: "UserSavedPermits",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSavedPermits");
        }
    }
}
