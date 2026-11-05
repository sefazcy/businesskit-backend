using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessKit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicToken",
                table: "Payments",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PublicToken",
                table: "Appointments",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            // Backfill existing rows with random 128-bit tokens (32 lowercase hex chars,
            // same format as PublicTokens.New()) before the unique indexes are created.
            migrationBuilder.Sql("UPDATE \"Payments\" SET \"PublicToken\" = lower(hex(randomblob(16))) WHERE \"PublicToken\" = '';");
            migrationBuilder.Sql("UPDATE \"Appointments\" SET \"PublicToken\" = lower(hex(randomblob(16))) WHERE \"PublicToken\" = '';");

            // Manual payments stored a checkout URL with the numeric payment id
            // (".../payment-status/{id}"), which no longer resolves. Clear them; the
            // public pages hide the "continue payment" link when it is null.
            migrationBuilder.Sql("UPDATE \"Payments\" SET \"ProviderCheckoutUrl\" = NULL WHERE \"Provider\" = 'Manual' AND \"ProviderCheckoutUrl\" LIKE '%/payment-status/%';");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PublicToken",
                table: "Payments",
                column: "PublicToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PublicToken",
                table: "Appointments",
                column: "PublicToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_PublicToken",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_PublicToken",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PublicToken",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PublicToken",
                table: "Appointments");
        }
    }
}
