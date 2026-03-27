using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using mvc.Parser.Models;

namespace mvc.Parser;

public interface IMediaWikiTokenizer
{
    IEnumerable<Token> Tokenize(string input);
}

public class MediaWikiTokenizer : IMediaWikiTokenizer
{
    private static readonly (string Key, TokenType Type)[] SimpleTokens = new[]
    {
        ("'''", TokenType.BoldStart),
        ("''", TokenType.ItalicStart),
        ("[[", TokenType.LinkStart),
        ("]]", TokenType.LinkEnd),
        ("[", TokenType.ExternalLinkStart),
        ("]", TokenType.ExternalLinkEnd),
        ("{{", TokenType.TemplateStart),
        ("}}", TokenType.TemplateEnd),
        ("{|", TokenType.TableStart),
        ("|}", TokenType.TableEnd),
        ("|-", TokenType.TableRow),
        ("|", TokenType.TableCell),
        ("!", TokenType.TableHeader),
        ("*", TokenType.BulletList),
        ("#", TokenType.NumberedList),
        ("\n", TokenType.NewLine)
    }.OrderByDescending(t => t.Item1.Length).ToArray();

    public IEnumerable<Token> Tokenize(string input)
    {
        if (string.IsNullOrEmpty(input)) yield break;

        int pos = 0;
        bool startOfLine = true;

        while (pos < input.Length)
        {
            bool matched = false;

            // ... (Categories section remains same)
            if (input.Length - pos >= 10)
            {
                string sub = input.Substring(pos).ToLower();
                if (sub.StartsWith("[[kategorie:"))
                {
                    int len = 12;
                    yield return new Token(TokenType.CategoryStart, input.Substring(pos, len), pos, len);
                    pos += len;
                    matched = true;
                    startOfLine = false;
                }
                else if (sub.StartsWith("[[category:"))
                {
                    int len = 11;
                    yield return new Token(TokenType.CategoryStart, input.Substring(pos, len), pos, len);
                    pos += len;
                    matched = true;
                    startOfLine = false;
                }
            }

            // Check for other simple tokens
            if (!matched)
            {
                foreach (var (key, type) in SimpleTokens)
                {
                    if (pos + key.Length <= input.Length && input.Substring(pos, key.Length) == key)
                    {
                        yield return new Token(type, key, pos, key.Length);
                        pos += key.Length;
                        matched = true;
                        if (type == TokenType.NewLine) startOfLine = true;
                        else startOfLine = false;
                        break;
                    }
                }
            }

            // Check for headings - ONLY at start of line OR if it's a trailing marker
            if (!matched)
            {
                bool isTrailing = false;
                if (!startOfLine && input[pos] == '=')
                {
                    int tempK = pos;
                    while (tempK < input.Length && input[tempK] == '=') tempK++;
                    int markerEnd = tempK;
                    while (tempK < input.Length && input[tempK] == ' ') tempK++;
                    if (tempK == input.Length || input[tempK] == '\n') isTrailing = true;
                }

                if (startOfLine || isTrailing)
                {
                    // Skip leading spaces for heading check at start of line
                    int spaceCount = 0;
                    if (startOfLine)
                    {
                        while (pos + spaceCount < input.Length && input[pos + spaceCount] == ' ')
                        {
                            spaceCount++;
                        }
                    }
                    
                    if (pos + spaceCount < input.Length && input[pos + spaceCount] == '=')
                    {
                        int count = 0;
                        while (pos + spaceCount + count < input.Length && input[pos + spaceCount + count] == '=')
                        {
                            count++;
                        }
                        if (count >= 1)
                        {
                            yield return new Token(TokenType.HeadingStart, input.Substring(pos + spaceCount, count), pos + spaceCount, count);
                            pos += spaceCount + count;
                            matched = true;
                            startOfLine = false;
                        }
                    }
                }
            }

            // Text token
            if (!matched)
            {
                int start = pos;
                // Move forward at least one character to avoid infinite loop
                pos++; 
                
                while (pos < input.Length)
                {
                    bool nextIsSpecial = false;
                    foreach (var (key, _) in SimpleTokens)
                    {
                        if (pos + key.Length <= input.Length && input.Substring(pos, key.Length) == key)
                        {
                            nextIsSpecial = true;
                            break;
                        }
                    }
                    if (nextIsSpecial) break;
                    
                    // Only stop for headings at start of line or if it looks like a trailing marker
                    if (input[pos] == '=')
                    {
                        // Check if it's a heading at start of line
                        if (pos == 0 || input[pos-1] == '\n') break;
                        
                        // Check if it's a trailing marker (followed by spaces and newline/end)
                        int tempK = pos;
                        while (tempK < input.Length && input[tempK] == '=') tempK++;
                        int markerEnd = tempK;
                        while (tempK < input.Length && input[tempK] == ' ') tempK++;
                        if (tempK == input.Length || input[tempK] == '\n') 
                        {
                            // This IS a trailing marker. Stop text token here.
                            break;
                        }
                    }
                    
                    pos++;
                }
                
                yield return new Token(TokenType.Text, input.Substring(start, pos - start), start, pos - start);
                matched = true;
                startOfLine = false;
            }
        }
    }
}
