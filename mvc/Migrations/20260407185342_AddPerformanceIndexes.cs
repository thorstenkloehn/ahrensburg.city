using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mvc.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WikiCategories_TenantId",
                table: "WikiCategories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_WikiArtikelVersions_TenantId_WikiArtikelId_Zeitpunkt",
                table: "WikiArtikelVersions",
                columns: new[] { "TenantId", "WikiArtikelId", "Zeitpunkt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WikiCategories_TenantId",
                table: "WikiCategories");

            migrationBuilder.DropIndex(
                name: "IX_WikiArtikelVersions_TenantId_WikiArtikelId_Zeitpunkt",
                table: "WikiArtikelVersions");
        }
    }
}
