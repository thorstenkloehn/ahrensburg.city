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
            ProcessInlineTokens(line, tokens);
            tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Newline });
        }

        return tokens;
    }

    private void ProcessInlineTokens(string line, List<MarkdownToken> tokens)
    {
        var current = line;
        
        // 1. Mark Categories: [[kategorie:Name]] -> [[CAT:Name]]
        current = Regex.Replace(current, @"\[\[[kK]ategorie:(.*?)\]\]", "[[CAT:$1]]");

        // 2. Mark Bold: **text** or __text__
        current = Regex.Replace(current, @"(\*\*|__)(.*?)\1", "[[BOLD:$2]]");
        
        // 3. Mark Italic: *text* or _text_
        current = Regex.Replace(current, @"(\*|_)(.*?)\1", "[[ITALIC:$2]]");
        
        // 4. Mark Links: [label](url)
        current = Regex.Replace(current, @"\[(.*?)\]\((.*?)\)", "[[LINK:$2|$1]]");

        // Split by all our markers
        var parts = Regex.Split(current, @"(\[\[CAT:.*?\]\]|\[\[BOLD:.*?\]\]|\[\[ITALIC:.*?\]\]|\[\[LINK:.*?\]\])");
        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part)) continue;

            if (part.StartsWith("[[CAT:"))
            {
                tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Category, Value = part[6..^2].Trim() });
            }
            else if (part.StartsWith("[[BOLD:"))
            {
                tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Bold, Value = part[7..^2] });
            }
            else if (part.StartsWith("[[ITALIC:"))
            {
                tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Italic, Value = part[9..^2] });
            }
            else if (part.StartsWith("[[LINK:"))
            {
                var linkContent = part[7..^2];
                var pipeIndex = linkContent.IndexOf('|');
                if (pipeIndex >= 0)
                {
                    tokens.Add(new MarkdownToken 
                    { 
                        Type = MarkdownTokenType.Link, 
                        Value = linkContent[..pipeIndex], 
                        Parameters = new List<string> { linkContent[(pipeIndex + 1)..] } 
                    });
                }
                else
                {
                    tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Link, Value = linkContent });
                }
            }
            else
            {
                tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Text, Value = part });
            }
        }
    }
}
