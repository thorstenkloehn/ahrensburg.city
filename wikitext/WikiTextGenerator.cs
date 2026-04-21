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

using Wikitext.Models;
using Wikitext.Parser;

namespace Wikitext;

/// <summary>
/// Provides high-level methods to generate WikiText from an Abstract Syntax Tree (AST).
/// </summary>
public static class WikiTextGenerator
{
    private static readonly IMediaWikiASTSerializer _serializer = new MediaWikiASTSerializer();

    /// <summary>
    /// Generates WikiText from the given WikiNode (AST).
    /// </summary>
    /// <param name="node">The root node or any node of the AST.</param>
    /// <returns>The generated WikiText string.</returns>
    public static string Generate(WikiNode node)
    {
        return _serializer.ToWikiText(node);
    }

    /// <summary>
    /// Generates HTML from the given WikiNode (AST).
    /// </summary>
    /// <param name="node">The root node or any node of the AST.</param>
    /// <returns>The generated HTML string.</returns>
    public static string GenerateHtml(WikiNode node)
    {
        return _serializer.ToHtml(node);
    }
}
