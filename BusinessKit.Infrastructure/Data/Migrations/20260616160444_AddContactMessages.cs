using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessKit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContactMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsReplied = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMessages", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactMessages");
        }
    }
}
