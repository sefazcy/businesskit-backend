using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessKit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApartmentManagementFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApartmentUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BlockName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    FloorNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    DoorNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    UnitType = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    GrossArea = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    NetArea = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    IsOccupied = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApartmentUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Residents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApartmentUnitId = table.Column<int>(type: "INTEGER", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Role = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    MoveInDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MoveOutDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Residents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Residents_ApartmentUnits_ApartmentUnitId",
                        column: x => x.ApartmentUnitId,
                        principalTable: "ApartmentUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApartmentUnits_BlockName_DoorNumber",
                table: "ApartmentUnits",
                columns: new[] { "BlockName", "DoorNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApartmentUnits_IsActive",
                table: "ApartmentUnits",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ApartmentUnits_IsOccupied",
                table: "ApartmentUnits",
                column: "IsOccupied");

            migrationBuilder.CreateIndex(
                name: "IX_Residents_ApartmentUnitId",
                table: "Residents",
                column: "ApartmentUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Residents_IsActive",
                table: "Residents",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Residents_Role",
                table: "Residents",
                column: "Role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Residents");

            migrationBuilder.DropTable(
                name: "ApartmentUnits");
        }
    }
}
