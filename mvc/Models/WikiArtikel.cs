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

/// <summary>
/// Repräsentiert einen Wiki-Artikel im System.
/// </summary>
[Index(nameof(TenantId), nameof(Slug), IsUnique = true)]
public class WikiArtikel
{
    /// <summary>
    /// Eindeutige ID des Artikels.
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Der Mandant, zu dem dieser Artikel gehört.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;
    
    /// <summary>
    /// Der eindeutige URL-Slug (Pfad) des Artikels.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    public int NamespaceId { get; set; } = 0; // Default namespace (Main)
    public WikiNamespace? Namespace { get; set; }

    /// <summary>
    /// Liste aller historischen und aktuellen Versionen dieses Artikels.
    /// </summary>
    public List<WikiArtikelVersion> Versionen { get; set; } = [];
}