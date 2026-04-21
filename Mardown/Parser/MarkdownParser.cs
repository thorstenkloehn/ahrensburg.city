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

using Mardown.Models;

namespace Mardown.Parser;

public class MarkdownParser
{
    private readonly MarkdownTokenizer _tokenizer;
    private readonly MarkdownASTBuilder _astBuilder;
    private readonly MarkdownASTSerializer _serializer;

    public MarkdownParser()
    {
        _tokenizer = new MarkdownTokenizer();
        _astBuilder = new MarkdownASTBuilder();
        _serializer = new MarkdownASTSerializer();
    }

    public string ToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return string.Empty;

        var tokens = _tokenizer.Tokenize(markdown);
        var ast = _astBuilder.Build(tokens, _tokenizer);
        return _serializer.ToHtml(ast);
    }

    public List<string> GetCategories(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return new List<string>();

        var tokens = _tokenizer.Tokenize(markdown);
        return tokens.Where(t => t.Type == MarkdownTokenType.Category).Select(t => t.Value).ToList();
    }
}
