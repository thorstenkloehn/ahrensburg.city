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

﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace mvc.Migrations
{
    /// <inheritdoc />
    public partial class thorsten_11_02_2026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WikiArtikels",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Slug = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikiArtikels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WikiArtikelVersions",
                columns: table => new
                {
                    VersionNummer = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarkdownInhalt = table.Column<string>(type: "text", nullable: false),
                    HtmlInhalt = table.Column<string>(type: "text", nullable: false),
                    Zeitpunkt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Kategorie = table.Column<List<string>>(type: "text[]", nullable: false),
                    WikiArtikelId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikiArtikelVersions", x => x.VersionNummer);
                    table.ForeignKey(
                        name: "FK_WikiArtikelVersions_WikiArtikels_WikiArtikelId",
                        column: x => x.WikiArtikelId,
                        principalTable: "WikiArtikels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WikiArtikelVersions_WikiArtikelId",
                table: "WikiArtikelVersions",
                column: "WikiArtikelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WikiArtikelVersions");

            migrationBuilder.DropTable(
                name: "WikiArtikels");
        }
    }
}
