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

using Mardown.Parser;
using Xunit;

namespace Mardown.Tests;

public class MarkdownParserTests
{
    private readonly MarkdownParser _parser = new();

    [Fact]
    public void ToHtml_Heading1_Works()
    {
        var result = _parser.ToHtml("# Headline");
        Assert.Equal("<h1>Headline</h1>\n", result);
    }

    [Fact]
    public void ToHtml_BoldAndItalic_Works()
    {
        var result = _parser.ToHtml("**Bold** and *Italic*");
        Assert.Equal("<p><strong>Bold</strong> and <em>Italic</em></p>\n", result);
    }

    [Fact]
    public void ToHtml_Link_Works()
    {
        var result = _parser.ToHtml("[Google](https://google.com)");
        Assert.Equal("<p><a href=\"https://google.com\">Google</a></p>\n", result);
    }

    [Fact]
    public void ToHtml_Template_Works()
    {
        var result = _parser.ToHtml("{{Info|Important Message}}");
        Assert.Equal("<div class=\"markdown-template\" data-name=\"Info\">Important Message</div>", result);
    }

    [Fact]
    public void ToHtml_List_Works()
    {
        var result = _parser.ToHtml("* Item 1\n* Item 2");
        Assert.Contains("<li>Item 1</li>", result);
        Assert.Contains("<li>Item 2</li>", result);
        Assert.Contains("<ul>", result);
    }

    [Fact]
    public void ToHtml_Category_IsInvisibleInHtml()
    {
        var result = _parser.ToHtml("Hallo Welt [[kategorie:Hauptseite]]");
        // With paragraphs it will be <p>Hallo Welt</p>\n
        Assert.Contains("Hallo Welt", result);
        Assert.DoesNotContain("Hauptseite", result);
    }

    [Fact]
    public void ToHtml_Code_Works()
    {
        // Inline Code
        var inlineResult = _parser.ToHtml("Benutze `var x = 1;`!");
        Assert.Equal("<p>Benutze <code>var x = 1;</code>!</p>\n", inlineResult);

        // Standard Code Block
        var blockResult = _parser.ToHtml("```\nConsole.WriteLine();\n```");
        Assert.Equal("<pre><code>Console.WriteLine();</code></pre>\n", blockResult);

        // Your Triple Single Quote Code Block
        var specialResult = _parser.ToHtml("'''\nSpecial Code\n'''");
        Assert.Equal("<pre><code>Special Code</code></pre>\n", specialResult);
    }

    [Fact]
    public void GetCategories_Works()
    {
        var categories = _parser.GetCategories("[[kategorie:A]]\n[[Kategorie:B]]");
        Assert.Equal(2, categories.Count);
        Assert.Contains("A", categories);
        Assert.Contains("B", categories);
    }
}
