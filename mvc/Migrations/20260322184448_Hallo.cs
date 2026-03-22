using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mvc.Migrations
{
    /// <inheritdoc />
    public partial class Hallo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WikiArtikelVersions_WikiArtikels_WikiArtikelId",
                table: "WikiArtikelVersions");

            migrationBuilder.AlterColumn<long>(
                name: "WikiArtikelId",
                table: "WikiArtikelVersions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MarkdownInhalt",
                table: "WikiArtikelVersions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "HtmlInhalt",
                table: "WikiArtikelVersions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_WikiArtikelVersions_WikiArtikels_WikiArtikelId",
                table: "WikiArtikelVersions",
                column: "WikiArtikelId",
                principalTable: "WikiArtikels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WikiArtikelVersions_WikiArtikels_WikiArtikelId",
                table: "WikiArtikelVersions");

            migrationBuilder.AlterColumn<long>(
                name: "WikiArtikelId",
                table: "WikiArtikelVersions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "MarkdownInhalt",
                table: "WikiArtikelVersions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HtmlInhalt",
                table: "WikiArtikelVersions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WikiArtikelVersions_WikiArtikels_WikiArtikelId",
                table: "WikiArtikelVersions",
                column: "WikiArtikelId",
                principalTable: "WikiArtikels",
                principalColumn: "Id");
        }
    }
}
