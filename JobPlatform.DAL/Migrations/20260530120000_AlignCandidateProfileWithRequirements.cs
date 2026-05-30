using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AlignCandidateProfileWithRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "candidate_profiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "candidate_profiles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Citizenship",
                table: "candidate_profiles",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryOfResidence",
                table: "candidate_profiles",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "candidate_profiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "candidate_profiles",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModerationStatus",
                table: "candidate_profiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "ModerationComment",
                table: "candidate_profiles",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModeratedByUserId",
                table: "candidate_profiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModeratedAt",
                table: "candidate_profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "candidate_languages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Level = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidate_languages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_candidate_languages_candidate_profiles_CandidateProfileId",
                        column: x => x.CandidateProfileId,
                        principalTable: "candidate_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_candidate_profiles_City",
                table: "candidate_profiles",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_profiles_CountryOfResidence",
                table: "candidate_profiles",
                column: "CountryOfResidence");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_profiles_DesiredPosition",
                table: "candidate_profiles",
                column: "DesiredPosition");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_profiles_JobSearchStatus",
                table: "candidate_profiles",
                column: "JobSearchStatus");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_profiles_ModerationStatus",
                table: "candidate_profiles",
                column: "ModerationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_languages_CandidateProfileId",
                table: "candidate_languages",
                column: "CandidateProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_languages_LanguageCode",
                table: "candidate_languages",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_languages_CandidateProfileId_LanguageCode",
                table: "candidate_languages",
                columns: new[] { "CandidateProfileId", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "candidate_languages");

            migrationBuilder.DropIndex(name: "IX_candidate_profiles_City", table: "candidate_profiles");
            migrationBuilder.DropIndex(name: "IX_candidate_profiles_CountryOfResidence", table: "candidate_profiles");
            migrationBuilder.DropIndex(name: "IX_candidate_profiles_DesiredPosition", table: "candidate_profiles");
            migrationBuilder.DropIndex(name: "IX_candidate_profiles_JobSearchStatus", table: "candidate_profiles");
            migrationBuilder.DropIndex(name: "IX_candidate_profiles_ModerationStatus", table: "candidate_profiles");

            migrationBuilder.DropColumn(name: "MiddleName", table: "candidate_profiles");
            migrationBuilder.DropColumn(name: "DateOfBirth", table: "candidate_profiles");
            migrationBuilder.DropColumn(name: "Citizenship", table: "candidate_profiles");
            migrationBuilder.DropColumn(name: "CountryOfResidence", table: "candidate_profiles");
            migrationBuilder.DropColumn(name: "Phone", table: "candidate_profiles");
            migrationBuilder.DropColumn(name: "Currency", table: "candidate_profiles");
            migrationBuilder.DropColumn(name: "ModerationStatus", table: "candidate_profiles");
            migrationBuilder.DropColumn(name: "ModerationComment", table: "candidate_profiles");
            migrationBuilder.DropColumn(name: "ModeratedByUserId", table: "candidate_profiles");
            migrationBuilder.DropColumn(name: "ModeratedAt", table: "candidate_profiles");
        }
    }
}