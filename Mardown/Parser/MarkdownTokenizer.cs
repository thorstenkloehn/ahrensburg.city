using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mardown.Models;
using System;
using System.Linq;

namespace Mardown.Parser;

public partial class MarkdownTokenizer
{
    // Pre-compiled Regex for better performance and Span support
    private static readonly Regex HeadingRegex = new(@"^(#{1,6})\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex ListRegex = new(@"^(\s*)([*+-]|\d+\.)\s+(.*)$", RegexOptions.Compiled);
    private static readonly Regex TemplateRegex = new(@"^\{\{(.*?)\}\}$", RegexOptions.Compiled);
    private static readonly Regex TableDividerRegex = new(@"^\|[\s\-\|:]+\|$", RegexOptions.Compiled);

    // Inline patterns
    private static readonly Regex CategoryRegex = new(@"\[\[[kK]ategorie:(.*?)\]\]", RegexOptions.Compiled);
    private static readonly Regex BoldRegex = new(@"(\*\*|__)(.*?)\1", RegexOptions.Compiled);
    private static readonly Regex ItalicRegex = new(@"(\*|_)(.*?)\1", RegexOptions.Compiled);
    private static readonly Regex LinkRegex = new(@"\[(.*?)\]\((.*?)\)", RegexOptions.Compiled);

    public List<MarkdownToken> Tokenize(string input)
    {
        if (string.IsNullOrEmpty(input)) return new List<MarkdownToken>();

        var tokens = new List<MarkdownToken>();
        ReadOnlySpan<char> inputSpan = input.AsSpan();

        // Zero-allocation line enumeration
        foreach (var line in inputSpan.EnumerateLines())
        {
            if (line.IsWhiteSpace())
            {
                tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Newline });
                continue;
            }

            // Headings using Span-aware Regex
            var headingMatch = HeadingRegex.Match(line.ToString()); // Match on string for simplicity, but Span-based soon
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

            // Tables (detect rows like | a | b |) using Span methods
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("|") && trimmedLine.EndsWith("|"))
            {
                if (TableDividerRegex.IsMatch(trimmedLine.ToString()) && trimmedLine.Contains("-".AsSpan(), StringComparison.Ordinal))
                {
                    tokens.Add(new MarkdownToken { Type = MarkdownTokenType.TableDivider });
                }
                else
                {
                    tokens.Add(new MarkdownToken 
                    { 
                        Type = MarkdownTokenType.TableRow, 
                        Value = trimmedLine.ToString() 
                    });
                }
                continue;
            }

            // Lists
            var listMatch = ListRegex.Match(line.ToString());
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

            // Templates
            var templateMatch = TemplateRegex.Match(line.ToString());
            if (templateMatch.Success)
            {
                var fullValue = templateMatch.Groups[1].Value;
                var parts = fullValue.Split('|');
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

    public void TokenizeInline(ReadOnlySpan<char> text, List<MarkdownToken> tokens)
    {
        if (text.IsEmpty) return;

        // Find earliest match using Span-aware approach
        (Match? match, MarkdownTokenType type) earliest = (null, MarkdownTokenType.Text);
        int earliestIndex = int.MaxValue;

        // Local function must take Span as parameter to avoid capture error (CS9108)
        void CheckMatch(ReadOnlySpan<char> t, Regex regex, MarkdownTokenType type, ref (Match? match, MarkdownTokenType type) earliestRes, ref int earliestIdx)
        {
            var m = regex.Match(t.ToString());
            if (m.Success && m.Index < earliestIdx)
            {
                earliestIdx = m.Index;
                earliestRes = (m, type);
            }
        }

        CheckMatch(text, CategoryRegex, MarkdownTokenType.Category, ref earliest, ref earliestIndex);
        CheckMatch(text, BoldRegex, MarkdownTokenType.Bold, ref earliest, ref earliestIndex);
        CheckMatch(text, ItalicRegex, MarkdownTokenType.Italic, ref earliest, ref earliestIndex);
        CheckMatch(text, LinkRegex, MarkdownTokenType.Link, ref earliest, ref earliestIndex);

        if (earliest.match == null)
        {
            tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Text, Value = text.ToString() });
            return;
        }

        // Add text before the match
        if (earliestIndex > 0)
        {
            tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Text, Value = text.Slice(0, earliestIndex).ToString() });
        }

        // Add the matched token
        switch (earliest.type)
        {
            case MarkdownTokenType.Category:
                tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Category, Value = earliest.match.Groups[1].Value.Trim() });
                break;
            case MarkdownTokenType.Bold:
                tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Bold, Value = earliest.match.Groups[2].Value });
                break;
            case MarkdownTokenType.Italic:
                tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Italic, Value = earliest.match.Groups[2].Value });
                break;
            case MarkdownTokenType.Link:
                tokens.Add(new MarkdownToken 
                { 
                    Type = MarkdownTokenType.Link, 
                    Value = earliest.match.Groups[2].Value, 
                    Parameters = new List<string> { earliest.match.Groups[1].Value } 
                });
                break;
        }

        // Recurse on the remaining text using Slice
        int nextStart = earliestIndex + earliest.match.Length;
        if (nextStart < text.Length)
        {
            TokenizeInline(text.Slice(nextStart), tokens);
        }
    }
}
