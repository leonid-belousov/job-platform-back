using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddVacancyRecruiters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "vacancy_recruiters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VacancyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecruiterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vacancy_recruiters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vacancy_recruiters_job_vacancies_VacancyId",
                        column: x => x.VacancyId,
                        principalTable: "job_vacancies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vacancy_recruiters_users_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vacancy_recruiters_users_RecruiterUserId",
                        column: x => x.RecruiterUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vacancy_recruiters_AssignedByUserId",
                table: "vacancy_recruiters",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_vacancy_recruiters_RecruiterUserId",
                table: "vacancy_recruiters",
                column: "RecruiterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_vacancy_recruiters_VacancyId",
                table: "vacancy_recruiters",
                column: "VacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_vacancy_recruiters_VacancyId_RecruiterUserId",
                table: "vacancy_recruiters",
                columns: new[] { "VacancyId", "RecruiterUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vacancy_recruiters");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "users");
        }
    }
}
