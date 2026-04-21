/*
 * ahrensburg.city (MeinCMS) - A lightweight CMS with Wiki functionality and multi-tenancy.
 * Copyright (C) 2026 Thorsten
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

﻿using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace mvc.Migrations
{
    /// <inheritdoc />
    public partial class MediaWikiSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WikiTextInhalt",
                table: "WikiArtikelVersions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NamespaceId",
                table: "WikiArtikels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "WikiCategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    ParentCategoryId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikiCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WikiCategories_WikiCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "WikiCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WikiNamespaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LocalizedName = table.Column<string>(type: "text", nullable: false),
                    IsContent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikiNamespaces", x => x.Id);
                });

            // Seed default namespace (Main) with ID 0
            migrationBuilder.Sql("INSERT INTO \"WikiNamespaces\" (\"Id\", \"Name\", \"LocalizedName\", \"IsContent\") OVERRIDING SYSTEM VALUE VALUES (0, 'Main', 'Haupt', true);");

            migrationBuilder.CreateIndex(
                name: "IX_WikiArtikels_NamespaceId",
                table: "WikiArtikels",
                column: "NamespaceId");

            migrationBuilder.CreateIndex(
                name: "IX_WikiCategories_ParentCategoryId",
                table: "WikiCategories",
                column: "ParentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_WikiArtikels_WikiNamespaces_NamespaceId",
                table: "WikiArtikels",
                column: "NamespaceId",
                principalTable: "WikiNamespaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WikiArtikels_WikiNamespaces_NamespaceId",
                table: "WikiArtikels");

            migrationBuilder.DropTable(
                name: "WikiCategories");

            migrationBuilder.DropTable(
                name: "WikiNamespaces");

            migrationBuilder.DropIndex(
                name: "IX_WikiArtikels_NamespaceId",
                table: "WikiArtikels");

            migrationBuilder.DropColumn(
                name: "WikiTextInhalt",
                table: "WikiArtikelVersions");

            migrationBuilder.DropColumn(
                name: "NamespaceId",
                table: "WikiArtikels");
        }
    }
}
