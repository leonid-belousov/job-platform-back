using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateCertificatesAndPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PhotoFileId",
                table: "candidate_profiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "candidate_profiles",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "candidate_certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Issuer = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CredentialId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CredentialUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidate_certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_candidate_certificates_candidate_profiles_CandidateProfileId",
                        column: x => x.CandidateProfileId,
                        principalTable: "candidate_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_candidate_profiles_PhotoFileId",
                table: "candidate_profiles",
                column: "PhotoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_certificates_CandidateProfileId",
                table: "candidate_certificates",
                column: "CandidateProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_certificates_Name",
                table: "candidate_certificates",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "candidate_certificates");

            migrationBuilder.DropIndex(
                name: "IX_candidate_profiles_PhotoFileId",
                table: "candidate_profiles");

            migrationBuilder.DropColumn(
                name: "PhotoFileId",
                table: "candidate_profiles");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "candidate_profiles");
        }
    }
}
