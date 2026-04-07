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
    private static readonly Regex TemplateRegex = new(@"\{\{(.*?)\}\}", RegexOptions.Compiled);
    private static readonly Regex TableDividerRegex = new(@"^\|[\s\-\|:]+\|$", RegexOptions.Compiled);

    // Code block detection
    private static readonly Regex CodeBlockStartRegex = new(@"^(\s*)(```|''')\s*(.*)$", RegexOptions.Compiled);

    // Inline patterns
    private static readonly Regex CategoryRegex = new(@"\[\[[kK]ategorie:(.*?)\]\]", RegexOptions.Compiled);
    private static readonly Regex CodeInlineRegex = new(@"`(.*?)`", RegexOptions.Compiled);
    private static readonly Regex BoldRegex = new(@"(\*\*|__)(.*?)\1", RegexOptions.Compiled);
    private static readonly Regex ItalicRegex = new(@"(\*|_)(.*?)\1", RegexOptions.Compiled);
    private static readonly Regex LinkRegex = new(@"\[(.*?)\]\((.*?)\)", RegexOptions.Compiled);

    public List<MarkdownToken> Tokenize(string input)
    {
        if (string.IsNullOrEmpty(input)) return new List<MarkdownToken>();

        var tokens = new List<MarkdownToken>();
        var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        bool inCodeBlock = false;
        string currentCodeMarker = "";
        string currentCodeContent = "";

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Code block handling
            if (inCodeBlock)
            {
                if (trimmedLine.EndsWith(currentCodeMarker) && trimmedLine.Length == currentCodeMarker.Length)
                {
                    tokens.Add(new MarkdownToken { Type = MarkdownTokenType.CodeBlock, Value = currentCodeContent.TrimEnd() });
                    inCodeBlock = false;
                    currentCodeContent = "";
                    continue;
                }
                currentCodeContent += line + "\n";
                continue;
            }

            var codeMatch = CodeBlockStartRegex.Match(line);
            if (codeMatch.Success)
            {
                inCodeBlock = true;
                currentCodeMarker = codeMatch.Groups[2].Value;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Newline });
                continue;
            }

            // Headings
            var headingMatch = HeadingRegex.Match(line);
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

            // Tables
            if (trimmedLine.StartsWith("|") && trimmedLine.EndsWith("|"))
            {
                if (TableDividerRegex.IsMatch(trimmedLine) && trimmedLine.Contains("-"))
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
            var listMatch = ListRegex.Match(line);
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
            var templateMatch = TemplateRegex.Match(line);
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
            TokenizeInline(line.AsSpan(), tokens);
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
        CheckMatch(text, CodeInlineRegex, MarkdownTokenType.CodeInline, ref earliest, ref earliestIndex);
        CheckMatch(text, TemplateRegex, MarkdownTokenType.Template, ref earliest, ref earliestIndex);
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
            case MarkdownTokenType.Template:
                var fullValue = earliest.match.Groups[1].Value;
                var tParts = fullValue.Split('|');
                tokens.Add(new MarkdownToken
                {
                    Type = MarkdownTokenType.Template,
                    Value = tParts[0].Trim(),
                    Parameters = tParts.Skip(1).Select(p => p.Trim()).ToList()
                });
                break;
            case MarkdownTokenType.Category:
                tokens.Add(new MarkdownToken { Type = MarkdownTokenType.Category, Value = earliest.match.Groups[1].Value.Trim() });
                break;
            case MarkdownTokenType.CodeInline:
                tokens.Add(new MarkdownToken { Type = MarkdownTokenType.CodeInline, Value = earliest.match.Groups[1].Value });
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
