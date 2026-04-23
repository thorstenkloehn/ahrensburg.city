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

using mvc.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiffPlex.DiffBuilder.Model;

namespace mvc.Services;

public interface IPageService
{
    bool IstSlugGueltig(string slug);
    Task<WikiArtikel?> GetArtikelMitNeuesterVersionAsync(string slug);
    Task<WikiArtikel?> GetArtikelMitHistorieAsync(string slug);
    Task ErstelleOderAktualisiereArtikelAsync(string slug, string markdownInhalt, List<string>? kategorien = null);
    Task ErstelleOderAktualisiereWikiArtikelAsync(string slug, string wikiTextInhalt, List<string>? kategorien = null);
    Task<List<WikiArtikel>> GetArtikelNachKategorieAsync(string kategorie);
    Task<List<WikiArtikel>> GetAllArtikelAsync();
    Task<List<WikiArtikel>> GetBlogArticlesAsync(int count = 10);
    Task<List<WikiArtikel>> GetNewsArticlesAsync(int count = 10);
    Task<bool> WiederherstellenAsync(long versionNummer);
    Task<WikiArtikelVersion?> GetVersionAsync(long versionNummer);
    Task<List<WikiArtikel>> SucheArtikelAsync(string query);
    DiffPaneModel GenerateDiff(string oldContent, string newContent);
}
