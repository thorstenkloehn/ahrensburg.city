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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace mvc.Models;

public class WikiNamespace
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. "Benutzer", "Kategorie", "Datei"
    public string LocalizedName { get; set; } = string.Empty;
    public bool IsContent { get; set; } = true;
}

public class WikiCategory
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    public long? ParentCategoryId { get; set; }
    public WikiCategory? ParentCategory { get; set; }
    public List<WikiCategory> SubCategories { get; set; } = [];
}
