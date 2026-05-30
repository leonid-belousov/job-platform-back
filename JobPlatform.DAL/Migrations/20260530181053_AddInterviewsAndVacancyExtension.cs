using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewsAndVacancyExtension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiresAt",
                table: "job_vacancies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExtendedAt",
                table: "job_vacancies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExtensionCount",
                table: "job_vacancies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "interview_invitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Format = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MeetingUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateRespondedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interview_invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_interview_invitations_job_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "job_applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_interview_invitations_ApplicationId",
                table: "interview_invitations",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_interview_invitations_ScheduledAt",
                table: "interview_invitations",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_interview_invitations_Status",
                table: "interview_invitations",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "interview_invitations");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "job_vacancies");

            migrationBuilder.DropColumn(
                name: "ExtendedAt",
                table: "job_vacancies");

            migrationBuilder.DropColumn(
                name: "ExtensionCount",
                table: "job_vacancies");
        }
    }
}
