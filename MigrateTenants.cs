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
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

class Program
{
    static void Main()
    {
        string inputPath = "meine_sicherung.xml";
        string outputPath = "meine_sicherung_migrated.xml";

        if (!File.Exists(inputPath))
        {
            Console.WriteLine($"Error: {inputPath} not found.");
            return;
        }

        XDocument doc = XDocument.Load(inputPath);
        var artikels = doc.Descendants("WikiArtikel").ToList();

        int docCount = 0;
        int mainCount = 0;

        foreach (var artikel in artikels)
        {
            string slug = artikel.Element("Slug")?.Value ?? "";
            string newTenantId = DetermineTenant(slug.AsSpan());

            // Update Artikel TenantId
            var tenantIdElement = artikel.Element("TenantId");
            if (tenantIdElement != null)
            {
                tenantIdElement.Value = newTenantId;
            }
            else
            {
                artikel.AddFirst(new XElement("TenantId", newTenantId));
            }

            // Update nested Versionen TenantId
            var versionen = artikel.Descendants("WikiArtikelVersion");
            foreach (var version in versionen)
            {
                var vTenantId = version.Element("TenantId");
                if (vTenantId != null)
                {
                    vTenantId.Value = newTenantId;
                }
                else
                {
                    version.AddFirst(new XElement("TenantId", newTenantId));
                }
            }

            if (newTenantId == "doc") docCount++;
            else mainCount++;
            
            // Console.WriteLine($"Migrated: {slug} -> {newTenantId}");
        }

        doc.Save(outputPath);
        Console.WriteLine("\nMigration Summary (SIMD & Span optimized):");
        Console.WriteLine($"Total: {artikels.Count}");
        Console.WriteLine($"To 'doc': {docCount}");
        Console.WriteLine($"To 'main': {mainCount}");
        Console.WriteLine($"Saved to: {outputPath}");
    }

    // SIMD-accelerated search for specific technical characters if needed
    // This is a demonstration of how SIMD could be used for character scanning
    static bool ContainsTechnicalMarker(ReadOnlySpan<char> slug)
    {
        // Example: Check if slug contains '/' or '+' or '#' using SIMD-like approach (via Vector)
        // Note: For short strings, the overhead might not be worth it, but for large buffers it is.
        return slug.ContainsAny('/', '+', '#');
    }

    static string DetermineTenant(ReadOnlySpan<char> slug)
    {
        // Technical topics (doc) - using Span-based checks to avoid string allocations
        string[] docPatterns = { 
            "AI/", "Ai/", "ASP NET Core/", "C++", "C-Sharp", "CMS/", "Golang", 
            "IDE", "Java", "JavaScript", "LMS/", "Python", "Rust", "Server/", 
            "Webframework/", "Schwachstellen", "MediaWiki", "DNS", "Postfix",
            "Golang", "Prompt"
        };

        foreach (var p in docPatterns)
        {
            if (slug.Contains(p.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                return "doc";
            }
        }

        return "main";
    }
}
