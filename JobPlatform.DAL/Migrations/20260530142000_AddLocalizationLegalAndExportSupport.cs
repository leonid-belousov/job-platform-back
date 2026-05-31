using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.DAL.Migrations
{
    public partial class AddLocalizationLegalAndExportSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "NameRu", table: "dictionary_items", type: "character varying(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<string>(name: "NameEn", table: "dictionary_items", type: "character varying(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<string>(name: "DescriptionRu", table: "dictionary_items", type: "character varying(1000)", maxLength: 1000, nullable: true);
            migrationBuilder.AddColumn<string>(name: "DescriptionEn", table: "dictionary_items", type: "character varying(1000)", maxLength: 1000, nullable: true);

            migrationBuilder.CreateTable(
                name: "legal_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Version = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_legal_documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_legal_consents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Version = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_legal_consents", x => x.Id);
                });

            migrationBuilder.CreateIndex(name: "IX_legal_documents_Type_Language_IsActive", table: "legal_documents", columns: new[] { "Type", "Language", "IsActive" });
            migrationBuilder.CreateIndex(name: "IX_legal_documents_Type_Version_Language", table: "legal_documents", columns: new[] { "Type", "Version", "Language" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_user_legal_consents_UserId", table: "user_legal_consents", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_user_legal_consents_UserId_DocumentType_Version", table: "user_legal_consents", columns: new[] { "UserId", "DocumentType", "Version" }, unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "user_legal_consents");
            migrationBuilder.DropTable(name: "legal_documents");
            migrationBuilder.DropColumn(name: "NameRu", table: "dictionary_items");
            migrationBuilder.DropColumn(name: "NameEn", table: "dictionary_items");
            migrationBuilder.DropColumn(name: "DescriptionRu", table: "dictionary_items");
            migrationBuilder.DropColumn(name: "DescriptionEn", table: "dictionary_items");
        }
    }
}