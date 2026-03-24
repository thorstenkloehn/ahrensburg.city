using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mvc.Migrations
{
    /// <inheritdoc />
    public partial class MultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WikiArtikels_Slug",
                table: "WikiArtikels");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "WikiArtikelVersions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "WikiArtikels",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_WikiArtikels_TenantId_Slug",
                table: "WikiArtikels",
                columns: new[] { "TenantId", "Slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WikiArtikels_TenantId_Slug",
                table: "WikiArtikels");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "WikiArtikelVersions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "WikiArtikels");

            migrationBuilder.CreateIndex(
                name: "IX_WikiArtikels_Slug",
                table: "WikiArtikels",
                column: "Slug",
                unique: true);
        }
    }
}
