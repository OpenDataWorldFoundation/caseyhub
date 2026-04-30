using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CaseyHub.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPermitChecker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermitAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NormalisedAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    ZoneCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OverlayCodes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BuildingTypeSlug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AnswersJson = table.Column<string>(type: "jsonb", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OutcomeReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TriggeredClauseNumbers = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AssessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermitAssessments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanningClauses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClauseNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OfficialUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningClauses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermitRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BuildingTypeId = table.Column<int>(type: "integer", nullable: false),
                    RuleType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    TriggerContextJson = table.Column<string>(type: "jsonb", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OutcomeReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PlanningClauseId = table.Column<int>(type: "integer", nullable: false),
                    ShortCircuitOnMatch = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermitRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermitRules_BuildingTypes_BuildingTypeId",
                        column: x => x.BuildingTypeId,
                        principalTable: "BuildingTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PermitRules_PlanningClauses_PlanningClauseId",
                        column: x => x.PlanningClauseId,
                        principalTable: "PlanningClauses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ZoneOverrideRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BuildingTypeId = table.Column<int>(type: "integer", nullable: false),
                    ZoneOrOverlayCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PrefixMatch = table.Column<bool>(type: "boolean", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OutcomeReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PlanningClauseId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZoneOverrideRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZoneOverrideRules_BuildingTypes_BuildingTypeId",
                        column: x => x.BuildingTypeId,
                        principalTable: "BuildingTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ZoneOverrideRules_PlanningClauses_PlanningClauseId",
                        column: x => x.PlanningClauseId,
                        principalTable: "PlanningClauses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RuleQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PermitRuleId = table.Column<int>(type: "integer", nullable: false),
                    FieldKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    QuestionText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HelpText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InputType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OptionsJson = table.Column<string>(type: "jsonb", nullable: true),
                    ValidationJson = table.Column<string>(type: "jsonb", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RuleQuestions_PermitRules_PermitRuleId",
                        column: x => x.PermitRuleId,
                        principalTable: "PermitRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingTypes_Slug",
                table: "BuildingTypes",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermitAssessments_AssessedAtUtc",
                table: "PermitAssessments",
                column: "AssessedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PermitAssessments_BuildingTypeSlug_Outcome",
                table: "PermitAssessments",
                columns: new[] { "BuildingTypeSlug", "Outcome" });

            migrationBuilder.CreateIndex(
                name: "IX_PermitAssessments_UserId",
                table: "PermitAssessments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PermitRules_BuildingTypeId",
                table: "PermitRules",
                column: "BuildingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PermitRules_PlanningClauseId",
                table: "PermitRules",
                column: "PlanningClauseId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningClauses_ClauseNumber",
                table: "PlanningClauses",
                column: "ClauseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RuleQuestions_PermitRuleId",
                table: "RuleQuestions",
                column: "PermitRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ZoneOverrideRules_BuildingTypeId",
                table: "ZoneOverrideRules",
                column: "BuildingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ZoneOverrideRules_PlanningClauseId",
                table: "ZoneOverrideRules",
                column: "PlanningClauseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermitAssessments");

            migrationBuilder.DropTable(
                name: "RuleQuestions");

            migrationBuilder.DropTable(
                name: "ZoneOverrideRules");

            migrationBuilder.DropTable(
                name: "PermitRules");

            migrationBuilder.DropTable(
                name: "BuildingTypes");

            migrationBuilder.DropTable(
                name: "PlanningClauses");
        }
    }
}
