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

namespace Wikitext.Parser;

public interface IMediaWikiParser
{
    string ToHtml(string wikiText);
    string ToWikiText(string html);
    List<string> GetCategories(string wikiText);
}

public class MediaWikiParser : IMediaWikiParser
{
    private readonly IMediaWikiTokenizer _tokenizer;
    private readonly IMediaWikiASTBuilder _astBuilder;
    private readonly IMediaWikiASTSerializer _serializer;

    public MediaWikiParser(
        IMediaWikiTokenizer tokenizer,
        IMediaWikiASTBuilder astBuilder,
        IMediaWikiASTSerializer serializer)
    {
        _tokenizer = tokenizer;
        _astBuilder = astBuilder;
        _serializer = serializer;
    }

    public string ToHtml(string wikiText)
    {
        var tokens = _tokenizer.Tokenize(wikiText);
        var ast = _astBuilder.Build(tokens);
        // Here we could add AST Transformers (e.g. for templates)
        return _serializer.ToHtml(ast);
    }

    public List<string> GetCategories(string wikiText)
    {
        var tokens = _tokenizer.Tokenize(wikiText);
        var ast = _astBuilder.Build(tokens);
        return ExtractCategories(ast);
    }

    private List<string> ExtractCategories(WikiNode node)
    {
        var categories = new List<string>();
        if (node is CategoryNode cat)
        {
            categories.Add(cat.CategoryName);
        }
        foreach (var child in node.Children)
        {
            categories.AddRange(ExtractCategories(child));
        }
        return categories;
    }

    public string ToWikiText(string html)
    {
        // For round-tripping, we would need an HTML-to-AST parser.
        // For now, this is a placeholder or simplified version.
        // In a real Parsoid-like system, this would be complex.
        return "Not implemented: HTML to WikiText requires an HTML-to-AST parser.";
    }
}
