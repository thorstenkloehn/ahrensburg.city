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
        while (pos < input.Length)
        {
            bool matched = false;

            // Check for categories
            if (input.Length - pos >= 10)
            {
                string sub = input.Substring(pos).ToLower();
                if (sub.StartsWith("[[kategorie:"))
                {
                    int len = 12;
                    yield return new Token(TokenType.CategoryStart, input.Substring(pos, len), pos, len);
                    pos += len;
                    matched = true;
                }
                else if (sub.StartsWith("[[category:"))
                {
                    int len = 11;
                    yield return new Token(TokenType.CategoryStart, input.Substring(pos, len), pos, len);
                    pos += len;
                    matched = true;
                }
            }

            // Check for headings
            if (!matched && input[pos] == '=')
            {
                int count = 0;
                while (pos + count < input.Length && input[pos + count] == '=')
                {
                    count++;
                }
                if (count >= 1)
                {
                    yield return new Token(TokenType.HeadingStart, input.Substring(pos, count), pos, count);
                    pos += count;
                    matched = true;
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
                        break;
                    }
                }
            }

            // Text token
            if (!matched)
            {
                int start = pos;
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
                    if (input[pos] == '=') break;
                    pos++;
                }
                if (pos > start)
                {
                    yield return new Token(TokenType.Text, input.Substring(start, pos - start), start, pos - start);
                    matched = true;
                }
            }
        }
    }
}
