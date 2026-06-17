using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessKit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffWorkingHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StaffWorkingHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StaffMemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    DayOfWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    EndTime = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    IsWorkingDay = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    BreakStartTime = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    BreakEndTime = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffWorkingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffWorkingHours_StaffMembers_StaffMemberId",
                        column: x => x.StaffMemberId,
                        principalTable: "StaffMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaffWorkingHours_DayOfWeek",
                table: "StaffWorkingHours",
                column: "DayOfWeek");

            migrationBuilder.CreateIndex(
                name: "IX_StaffWorkingHours_StaffMemberId",
                table: "StaffWorkingHours",
                column: "StaffMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffWorkingHours_StaffMemberId_DayOfWeek",
                table: "StaffWorkingHours",
                columns: new[] { "StaffMemberId", "DayOfWeek" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffWorkingHours");
        }
    }
}
