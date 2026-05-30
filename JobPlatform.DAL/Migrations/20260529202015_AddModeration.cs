using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModeratedAt",
                table: "job_vacancies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModeratedByUserId",
                table: "job_vacancies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModerationComment",
                table: "job_vacancies",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModerationStatus",
                table: "job_vacancies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModeratedAt",
                table: "companies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModeratedByUserId",
                table: "companies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModerationComment",
                table: "companies",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModerationStatus",
                table: "companies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_job_vacancies_ModerationStatus",
                table: "job_vacancies",
                column: "ModerationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_companies_ModerationStatus",
                table: "companies",
                column: "ModerationStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_job_vacancies_ModerationStatus",
                table: "job_vacancies");

            migrationBuilder.DropIndex(
                name: "IX_companies_ModerationStatus",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "ModeratedAt",
                table: "job_vacancies");

            migrationBuilder.DropColumn(
                name: "ModeratedByUserId",
                table: "job_vacancies");

            migrationBuilder.DropColumn(
                name: "ModerationComment",
                table: "job_vacancies");

            migrationBuilder.DropColumn(
                name: "ModerationStatus",
                table: "job_vacancies");

            migrationBuilder.DropColumn(
                name: "ModeratedAt",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "ModeratedByUserId",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "ModerationComment",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "ModerationStatus",
                table: "companies");
        }
    }
}
