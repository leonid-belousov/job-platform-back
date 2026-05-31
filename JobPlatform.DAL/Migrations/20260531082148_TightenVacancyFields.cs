using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.DAL.Migrations
{
    /// <inheritdoc />
    public partial class TightenVacancyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_job_vacancies_Status_City",
                table: "job_vacancies");

            migrationBuilder.AlterColumn<string>(
                name: "Requirements",
                table: "job_vacancies",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(8000)",
                oldMaxLength: 8000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "job_vacancies",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Conditions",
                table: "job_vacancies",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(8000)",
                oldMaxLength: 8000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_job_vacancies_Country",
                table: "job_vacancies",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_job_vacancies_Status_Country_City",
                table: "job_vacancies",
                columns: new[] { "Status", "Country", "City" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_job_vacancies_Country",
                table: "job_vacancies");

            migrationBuilder.DropIndex(
                name: "IX_job_vacancies_Status_Country_City",
                table: "job_vacancies");

            migrationBuilder.AlterColumn<string>(
                name: "Requirements",
                table: "job_vacancies",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(8000)",
                oldMaxLength: 8000);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "job_vacancies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(80)",
                oldMaxLength: 80);

            migrationBuilder.AlterColumn<string>(
                name: "Conditions",
                table: "job_vacancies",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(8000)",
                oldMaxLength: 8000);

            migrationBuilder.CreateIndex(
                name: "IX_job_vacancies_Status_City",
                table: "job_vacancies",
                columns: new[] { "Status", "City" });
        }
    }
}
