using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.DAL.Migrations
{
    public partial class AddCandidateProfileCompletenessAndSkills : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasNoExperience",
                table: "candidate_profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsComplete",
                table: "candidate_profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAt",
                table: "candidate_profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "candidate_skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillCode = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidate_skills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_candidate_skills_candidate_profiles_CandidateProfileId",
                        column: x => x.CandidateProfileId,
                        principalTable: "candidate_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_candidate_profiles_IsComplete",
                table: "candidate_profiles",
                column: "IsComplete");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_skills_SkillCode",
                table: "candidate_skills",
                column: "SkillCode");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_skills_CandidateProfileId_SkillCode",
                table: "candidate_skills",
                columns: new[] { "CandidateProfileId", "SkillCode" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "candidate_skills");
            migrationBuilder.DropIndex(name: "IX_candidate_profiles_IsComplete", table: "candidate_profiles");
            migrationBuilder.DropColumn(name: "HasNoExperience", table: "candidate_profiles");
            migrationBuilder.DropColumn(name: "IsComplete", table: "candidate_profiles");
            migrationBuilder.DropColumn(name: "CompletedAt", table: "candidate_profiles");
        }
    }
}