using JobPlatform.DAL.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.DAL.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260620090000_AddContentPages")]
public partial class AddContentPages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            create table if not exists content_pages (
                "Id" uuid not null primary key,
                "Slug" character varying(120) not null,
                "SchemaVersion" integer not null,
                "JsonContent" jsonb not null,
                "IsPublished" boolean not null,
                "PublishedAt" timestamp with time zone null,
                "UpdatedByUserId" uuid null,
                "CreatedAt" timestamp with time zone not null,
                "UpdatedAt" timestamp with time zone null,
                "DeletedAt" timestamp with time zone null,
                "IsDeleted" boolean not null
            );
            create unique index if not exists "IX_content_pages_Slug" on content_pages ("Slug");
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("drop table if exists content_pages;");
    }
}
