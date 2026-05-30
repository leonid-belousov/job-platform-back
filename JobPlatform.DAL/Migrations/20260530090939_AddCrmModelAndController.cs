using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmModelAndController : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "crm_leads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Source = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CandidateProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    VacancyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crm_leads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_crm_leads_users_ResponsibleUserId",
                        column: x => x.ResponsibleUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "crm_activities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crm_activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_crm_activities_crm_leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "crm_leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crm_tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crm_tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_crm_tasks_crm_leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "crm_leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_crm_tasks_users_ResponsibleUserId",
                        column: x => x.ResponsibleUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_crm_activities_CreatedAt",
                table: "crm_activities",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_crm_activities_LeadId",
                table: "crm_activities",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_crm_activities_Type",
                table: "crm_activities",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_crm_leads_ApplicationId",
                table: "crm_leads",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_crm_leads_CandidateProfileId",
                table: "crm_leads",
                column: "CandidateProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_crm_leads_CompanyId",
                table: "crm_leads",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_crm_leads_CreatedAt",
                table: "crm_leads",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_crm_leads_ResponsibleUserId",
                table: "crm_leads",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_crm_leads_Status",
                table: "crm_leads",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_crm_leads_Type",
                table: "crm_leads",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_crm_leads_VacancyId",
                table: "crm_leads",
                column: "VacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_crm_tasks_DueDate",
                table: "crm_tasks",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_crm_tasks_LeadId",
                table: "crm_tasks",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_crm_tasks_ResponsibleUserId",
                table: "crm_tasks",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_crm_tasks_Status",
                table: "crm_tasks",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "crm_activities");

            migrationBuilder.DropTable(
                name: "crm_tasks");

            migrationBuilder.DropTable(
                name: "crm_leads");
        }
    }
}
