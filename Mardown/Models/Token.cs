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

namespace Mardown.Models;

public enum MarkdownTokenType
{
    Text,
    Heading,
    Bold,
    Italic,
    Link,
    Template,
    List,
    Newline,
    Category,
    TableRow,
    TableDivider,
    CodeInline,
    CodeBlock
}

public class MarkdownToken
{
    public MarkdownTokenType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public int Level { get; set; } // For headings or nested lists
    public List<string> Parameters { get; set; } = new(); // For templates
}
