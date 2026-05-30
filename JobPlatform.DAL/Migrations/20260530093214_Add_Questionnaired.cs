using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Add_Questionnaired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "questionnaires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionnaires", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "questionnaire_responses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionnaire_responses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_questionnaire_responses_questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questionnaire_sections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionnaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionnaire_sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_questionnaire_sections_questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questionnaire_questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    QuestionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    Placeholder = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HelpText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ValidationRules = table.Column<string>(type: "jsonb", nullable: true),
                    VisibilityCondition = table.Column<string>(type: "jsonb", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionnaire_questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_questionnaire_questions_questionnaire_sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "questionnaire_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questionnaire_answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "jsonb", nullable: true),
                    FileId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionnaire_answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_questionnaire_answers_questionnaire_questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "questionnaire_questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_questionnaire_answers_questionnaire_responses_ResponseId",
                        column: x => x.ResponseId,
                        principalTable: "questionnaire_responses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questionnaire_options",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionnaire_options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_questionnaire_options_questionnaire_questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "questionnaire_questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_answers_FileId",
                table: "questionnaire_answers",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_answers_QuestionId",
                table: "questionnaire_answers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_answers_ResponseId",
                table: "questionnaire_answers",
                column: "ResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_options_QuestionId",
                table: "questionnaire_options",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_options_QuestionId_SortOrder",
                table: "questionnaire_options",
                columns: new[] { "QuestionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_questions_SectionId",
                table: "questionnaire_questions",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_questions_SectionId_SortOrder",
                table: "questionnaire_questions",
                columns: new[] { "SectionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_responses_EntityType_EntityId",
                table: "questionnaire_responses",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_responses_QuestionnaireId",
                table: "questionnaire_responses",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_responses_SubmittedAt",
                table: "questionnaire_responses",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_responses_UserId",
                table: "questionnaire_responses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_sections_QuestionnaireId",
                table: "questionnaire_sections",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_questionnaire_sections_QuestionnaireId_SortOrder",
                table: "questionnaire_sections",
                columns: new[] { "QuestionnaireId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_questionnaires_CreatedByUserId",
                table: "questionnaires",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_questionnaires_EntityId",
                table: "questionnaires",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_questionnaires_EntityType",
                table: "questionnaires",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_questionnaires_IsActive",
                table: "questionnaires",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "questionnaire_answers");

            migrationBuilder.DropTable(
                name: "questionnaire_options");

            migrationBuilder.DropTable(
                name: "questionnaire_responses");

            migrationBuilder.DropTable(
                name: "questionnaire_questions");

            migrationBuilder.DropTable(
                name: "questionnaire_sections");

            migrationBuilder.DropTable(
                name: "questionnaires");
        }
    }
}
