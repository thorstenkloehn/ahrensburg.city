using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mardown.Models;

namespace Mardown.Parser;

public class MarkdownTokenizer
{
    public List<MarkdownToken> Tokenize(string input)
    {
        var tokens = new List<MarkdownToken>();
        var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Newline });
                continue;
            }

            // Headings
            var headingMatch = Regex.Match(line, @"^(#{1,6})\s+(.*)$");
            if (headingMatch.Success)
            {
                tokens.Add(new MarkdownToken
                {
                    Type = MarkdownTokenType.Heading,
                    Level = headingMatch.Groups[1].Length,
                    Value = headingMatch.Groups[2].Value
                });
                continue;
            }

            // Tables (detect rows like | a | b |)
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith('|') && trimmedLine.EndsWith('|'))
            {
                // Check if it is a divider like |---| or | :--- | ---: |
                // Standard GFM divider: at least one dash per cell (common requirement).
                // We'll look for lines that only contain |, -, :, and whitespace, and have at least one dash.
                if (Regex.IsMatch(trimmedLine, @"^\|[\s\-\|:]+\|$") && trimmedLine.Contains('-'))
                {
                    tokens.Add(new MarkdownToken { Type = MarkdownTokenType.TableDivider });
                }
                else
                {
                    tokens.Add(new MarkdownToken 
                    { 
                        Type = MarkdownTokenType.TableRow, 
                        Value = trimmedLine 
                    });
                }
                continue;
            }

            // Lists
            var listMatch = Regex.Match(line, @"^(\s*)([*+-]|\d+\.)\s+(.*)$");
            if (listMatch.Success)
            {
                tokens.Add(new MarkdownToken
                {
                    Type = MarkdownTokenType.List,
                    Level = listMatch.Groups[1].Length / 2, // Indentation level
                    Value = listMatch.Groups[3].Value
                });
                continue;
            }

            // Templates (e.g. {{TemplateName|param1|param2}})
            var templateMatch = Regex.Match(line, @"^\{\{(.*?)\}\}$");
            if (templateMatch.Success)
            {
                var parts = templateMatch.Groups[1].Value.Split('|');
                tokens.Add(new MarkdownToken
                {
                    Type = MarkdownTokenType.Template,
                    Value = parts[0],
                    Parameters = parts.Skip(1).ToList()
                });
                continue;
            }

            // Standard line (could contain bold, italic, links, and categories)
            TokenizeInline(line, tokens);
            tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Newline });
        }

        return tokens;
    }

    public void TokenizeInline(string text, List<MarkdownToken> tokens)
    {
        if (string.IsNullOrEmpty(text)) return;

        // Find the first match among all possible inline patterns
        var patterns = new (string Pattern, MarkdownTokenType Type)[] 
        { 
            (@"\[\[[kK]ategorie:(.*?)\]\]", MarkdownTokenType.Category),
            (@"(\*\*|__)(.*?)\1", MarkdownTokenType.Bold),
            (@"(\*|_)(.*?)\1", MarkdownTokenType.Italic),
            (@"\[(.*?)\]\((.*?)\)", MarkdownTokenType.Link)
        };

        // Find earliest match
        Match? earliestMatch = null;
        MarkdownTokenType matchedType = MarkdownTokenType.Text;

        foreach (var p in patterns)
        {
            var match = Regex.Match(text, p.Pattern);
            if (match.Success && (earliestMatch == null || match.Index < earliestMatch.Index))
            {
                earliestMatch = match;
                matchedType = p.Type;
            }
        }

        if (earliestMatch == null)
        {
            tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Text, Value = text });
            return;
        }

        // Add text before the match
        if (earliestMatch.Index > 0)
        {
            tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Text, Value = text[..earliestMatch.Index] });
        }

        // Add the matched token
        switch (matchedType)
        {
            case MarkdownTokenType.Category:
                tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Category, Value = earliestMatch.Groups[1].Value.Trim() });
                break;
            case MarkdownTokenType.Bold:
                tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Bold, Value = earliestMatch.Groups[2].Value });
                break;
            case MarkdownTokenType.Italic:
                tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Italic, Value = earliestMatch.Groups[2].Value });
                break;
            case MarkdownTokenType.Link:
                tokens.Add(new MarkdownToken 
                { 
                    Type = MarkdownTokenType.Link, 
                    Value = earliestMatch.Groups[2].Value, 
                    Parameters = new List<string> { earliestMatch.Groups[1].Value } 
                });
                break;
        }

        // Recurse on the remaining text
        TokenizeInline(text[(earliestMatch.Index + earliestMatch.Length)..], tokens);
    }
}
