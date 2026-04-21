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

using System;
using System.ComponentModel.DataAnnotations;

namespace mvc.Models;

public class WikiArtikelVersion
{

    [Key]
    public long VersionNummer { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string? MarkdownInhalt { get; set; }
    public string? WikiTextInhalt { get; set; }

    [System.Xml.Serialization.XmlIgnore]
    [YamlDotNet.Serialization.YamlIgnore]
    public string? HtmlInhalt { get; set; }
    public DateTime Zeitpunkt { get; set; }
    public List<string> Kategorie { get; set; } = [];

    // Fremdschlüssel zum zugehörigen WikiArtikel
    public long WikiArtikelId { get; set; }
    
    [System.Xml.Serialization.XmlIgnore]
    [YamlDotNet.Serialization.YamlIgnore]
    public WikiArtikel? WikiArtikel { get; set; }

}
