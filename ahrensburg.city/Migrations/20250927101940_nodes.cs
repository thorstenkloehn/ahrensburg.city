using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ahrensburg.city.Migrations
{
    /// <inheritdoc />
    public partial class nodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Nachrichten");

            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titel = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Beschreibung = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HtmlInhalt = table.Column<string>(type: "text", nullable: false),
                    Mardown = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Nodes");

            migrationBuilder.CreateTable(
                name: "Nachrichten",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Beschreibung = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HtmlInhalt = table.Column<string>(type: "text", nullable: false),
                    Mardown = table.Column<string>(type: "text", nullable: false),
                    Titel = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nachrichten", x => x.Id);
                });
        }
    }
}
