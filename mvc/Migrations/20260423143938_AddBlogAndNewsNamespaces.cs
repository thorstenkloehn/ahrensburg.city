using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mvc.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogAndNewsNamespaces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO \"WikiNamespaces\" (\"Id\", \"Name\", \"LocalizedName\", \"IsContent\") OVERRIDING SYSTEM VALUE VALUES (100, 'Blog', 'Blog', true);");
            migrationBuilder.Sql("INSERT INTO \"WikiNamespaces\" (\"Id\", \"Name\", \"LocalizedName\", \"IsContent\") OVERRIDING SYSTEM VALUE VALUES (101, 'News', 'News', true);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"WikiNamespaces\" WHERE \"Id\" IN (100, 101);");
        }
    }
}
