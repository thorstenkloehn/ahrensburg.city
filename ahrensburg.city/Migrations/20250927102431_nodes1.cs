using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ahrensburg.city.Migrations
{
    /// <inheritdoc />
    public partial class nodes1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Beschreibung",
                table: "Nodes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Beschreibung",
                table: "Nodes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
