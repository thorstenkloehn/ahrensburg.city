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

using Wikitext;
using Wikitext.Models;
using Xunit;

namespace mvc.Tests;

public class WikiTextGeneratorTests
{
    [Fact]
    public void Should_Generate_Simple_WikiText()
    {
        // Arrange
        var root = new RootNode();
        root.Children.Add(new HeadingNode { Level = 1, Children = { new TextNode { Text = "Title" } } });
        root.Children.Add(new ParagraphNode { Children = { new TextNode { Text = "Hello " }, new BoldNode { Children = { new TextNode { Text = "World" } } } } });

        // Act
        var result = WikiTextGenerator.Generate(root);

        // Assert
        Assert.Contains("=Title=", result);
        Assert.Contains("'''World'''", result);
    }

    [Fact]
    public void Should_Generate_Table_WikiText()
    {
        // Arrange
        var table = new TableNode();
        var row = new TableRowNode();
        row.Children.Add(new TableCellNode { IsHeader = true, Children = { new TextNode { Text = "H1" } } });
        row.Children.Add(new TableCellNode { IsHeader = false, Children = { new TextNode { Text = "C1" } } });
        table.Children.Add(row);

        // Act
        var result = WikiTextGenerator.Generate(table);

        // Assert
        Assert.Contains("{|", result);
        Assert.Contains("|-", result);
        Assert.Contains("! H1", result);
        Assert.Contains("| C1", result);
        Assert.Contains("|}", result);
    }

    [Fact]
    public void Should_Generate_Category_And_Template()
    {
        // Arrange
        var root = new RootNode();
        root.Children.Add(new CategoryNode { CategoryName = "Test", SortKey = "Key" });
        root.Children.Add(new TemplateNode 
        { 
            TemplateName = "Infobox", 
            Parameters = new Dictionary<string, string> { { "name", "value" }, { "1", "anon" } } 
        });

        // Act
        var result = WikiTextGenerator.Generate(root);

        // Assert
        Assert.Contains("[[Kategorie:Test|Key]]", result);
        Assert.Contains("{{Infobox|name=value|anon}}", result);
    }
}
