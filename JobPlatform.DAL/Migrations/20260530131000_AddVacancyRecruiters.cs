using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.DAL.Migrations
{
    public partial class AddVacancyRecruiters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_vacancy_recruiters_VacancyId",
                table: "vacancy_recruiters",
                column: "VacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_vacancy_recruiters_RecruiterUserId",
                table: "vacancy_recruiters",
                column: "RecruiterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_vacancy_recruiters_VacancyId_RecruiterUserId",
                table: "vacancy_recruiters",
                columns: new[] { "VacancyId", "RecruiterUserId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "vacancy_recruiters");
        }
    }
}