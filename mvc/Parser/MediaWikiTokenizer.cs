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
        ("||", TokenType.TableCell),
        ("!!", TokenType.TableHeader),
        ("|", TokenType.TableCell),
        ("!", TokenType.TableHeader),
        ("*", TokenType.BulletList),
        ("#", TokenType.NumberedList),
        ("\n", TokenType.NewLine),
        ("<code>", TokenType.CodeStart),
        ("</code>", TokenType.CodeEnd)
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
                
                // Fallback check for \r as NewLine if it's not followed by \n (which is in SimpleTokens)
                if (!matched && pos < input.Length && input[pos] == '\r')
                {
                    yield return new Token(TokenType.NewLine, "\r", pos, 1);
                    pos++;
                    matched = true;
                    startOfLine = true;
                }
            }

            // Check for headings - ONLY at start of line OR if it's a trailing marker
            if (!matched)
            {
                bool isTrailing = false;
                int markerPos = pos;
                if (!startOfLine && input[pos] == '=')
                {
                    int tempK = pos;
                    while (tempK < input.Length && input[tempK] == '=') tempK++;
                    int markerEnd = tempK;
                    while (tempK < input.Length && input[tempK] == ' ') tempK++;
                    if (tempK == input.Length || input[tempK] == '\n' || input[tempK] == '\r') isTrailing = true;
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
                // DO NOT pos++ here immediately, check first char too
                
                while (pos < input.Length)
                {
                    // Check if current position starts a special token
                    bool currentIsSpecial = false;
                    foreach (var (key, _) in SimpleTokens)
                    {
                        if (pos + key.Length <= input.Length && input.Substring(pos, key.Length) == key)
                        {
                            currentIsSpecial = true;
                            break;
                        }
                    }
                    if (!currentIsSpecial && input[pos] == '\r') currentIsSpecial = true; // Handle lone \r
                    
                    if (currentIsSpecial && pos > start) break;

                    // Only stop for headings at start of line or if it looks like a trailing marker
                    if (input[pos] == '=')
                    {
                        // Check if it's a heading at start of line
                        if (pos == 0 || input[pos-1] == '\n' || input[pos-1] == '\r') 
                        {
                            if (pos > start) break;
                        }
                        
                        // Check if it's a trailing marker (followed by spaces and newline/end)
                        int tempK = pos;
                        while (tempK < input.Length && input[tempK] == '=') tempK++;
                        int markerEnd = tempK;
                        while (tempK < input.Length && input[tempK] == ' ') tempK++;
                        if (tempK == input.Length || input[tempK] == '\n' || input[tempK] == '\r') 
                        {
                            // This IS a trailing marker. Stop text token here.
                            if (pos > start) break;
                        }
                    }

                    // Stop at other special multi-char tokens like !! or ||
                    if (pos + 2 <= input.Length)
                    {
                        string next2 = input.Substring(pos, 2);
                        if ((next2 == "!!" || next2 == "||") && pos > start) break;
                    }
                    
                    pos++;
                    if (currentIsSpecial) break; // If we just processed a single-char special token that didn't trigger 'pos > start'
                }
                
                if (pos > start)
                {
                    yield return new Token(TokenType.Text, input.Substring(start, pos - start), start, pos - start);
                    matched = true;
                    startOfLine = false;
                }
            }
        }
    }
}
