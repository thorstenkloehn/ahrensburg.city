using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mvc.Migrations
{
    /// <inheritdoc />
    public partial class SearchIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"CREATE INDEX ""IX_WikiArtikelVersion_Search"" ON ""WikiArtikelVersions"" USING GIN (
                    to_tsvector('german', 
                        coalesce(""HtmlInhalt"", '') || ' ' || 
                        coalesce(""MarkdownInhalt"", '') || ' ' || 
                        coalesce(""WikiTextInhalt"", '')
                    )
                );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX ""IX_WikiArtikelVersion_Search"";");
        }
    }
}
