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
            string newTenantId = DetermineTenant(slug);

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
            
            Console.WriteLine($"Migrated: {slug} -> {newTenantId}");
        }

        doc.Save(outputPath);
        Console.WriteLine("\nMigration Summary:");
        Console.WriteLine($"Total: {artikels.Count}");
        Console.WriteLine($"To 'doc': {docCount}");
        Console.WriteLine($"To 'main': {mainCount}");
        Console.WriteLine($"Saved to: {outputPath}");
    }

    static string DetermineTenant(string slug)
    {
        // Technical topics (doc)
        string[] docPatterns = { 
            "AI/", "Ai/", "ASP NET Core/", "C++", "C-Sharp", "CMS/", "Golang", 
            "IDE", "Java", "JavaScript", "LMS/", "Python", "Rust", "Server/", 
            "Webframework/", "Schwachstellen", "MediaWiki", "DNS", "Postfix",
            "Golang", "Prompt"
        };

        if (docPatterns.Any(p => slug.Contains(p, StringComparison.OrdinalIgnoreCase)))
        {
            return "doc";
        }

        // Default for everything else (Ahrensburg related or Blogs)
        return "main";
    }
}
