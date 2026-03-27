using Xunit;
using mvc.Parser;
using mvc.Parser.Models;
using System.Collections.Generic;
using System.Linq;

namespace mvc.Tests;

public class MediaWikiParserTests
{
    private readonly IMediaWikiParser _parser;

    public MediaWikiParserTests()
    {
        var tokenizer = new MediaWikiTokenizer();
        var astBuilder = new MediaWikiASTBuilder();
        var serializer = new MediaWikiASTSerializer();
        _parser = new MediaWikiParser(tokenizer, astBuilder, serializer);
    }

    [Fact]
    public void TestBasicParsing()
    {
        string wikiText = "== Heading ==\nThis is '''bold''' and ''italic''.\n[[Target|Link]]";
        string html = _parser.ToHtml(wikiText);
        
        // Output for debugging
        System.Console.WriteLine("DEBUG OUTPUT: " + html);

        Assert.Contains("<h2 data-mw=\"heading\"> Heading </h2>", html);
        Assert.Contains("<b data-mw=\"bold\">bold</b>", html);
        Assert.Contains("<i data-mw=\"italic\">italic</i>", html);
        Assert.Contains("<a href=\"/Target\" data-mw=\"link\">Link</a>", html);
    }

    [Fact]
    public void TestTokenizerDetailed()
    {
        var tokenizer = new MediaWikiTokenizer();
        var tokens = tokenizer.Tokenize("[[Target|Link]]").ToList();

        foreach (var t in tokens)
        {
            System.Console.WriteLine($"TOKEN: {t}");
        }

        Assert.Equal(TokenType.LinkStart, tokens[0].Type);
        Assert.Equal(TokenType.Text, tokens[1].Type); // Target
        Assert.Equal(TokenType.TableCell, tokens[2].Type); // |
        Assert.Equal(TokenType.Text, tokens[3].Type); // Link
        Assert.Equal(TokenType.LinkEnd, tokens[4].Type);
    }

    [Fact]
    public void TestLists()
    {
        string wikiText = "* Item 1\n* Item 2\n# Ordered 1\n# Ordered 2";
        string html = _parser.ToHtml(wikiText);

        Assert.Contains("<ul><li> Item 1</li><li> Item 2</li></ul>", html.Replace("\n", ""));
        Assert.Contains("<ol><li> Ordered 1</li><li> Ordered 2</li></ol>", html.Replace("\n", ""));
    }

    [Fact]
    public void TestCategories()
    {
        string wikiText = "Content\n[[Kategorie:Auto]]\n[[Category:Programming]]";
        var categories = _parser.GetCategories(wikiText);

        Assert.Contains("Auto", categories);
        Assert.Contains("Programming", categories);
        
        string html = _parser.ToHtml(wikiText);
        Assert.DoesNotContain("Kategorie:Auto", html);
    }

    [Fact]
    public void TestExternalLinks()
    {
        string wikiText = "[https://www.block-house.de/restaurants/ahrensburg/grosse-strasse/ BLOCK HOUSE Ahrensburg]";
        string html = _parser.ToHtml(wikiText);

        Assert.Contains("<a href=\"https://www.block-house.de/restaurants/ahrensburg/grosse-strasse/\" data-mw=\"link\">BLOCK HOUSE Ahrensburg</a>", html);
    }

    [Fact]
    public void TestTables()
    {
        string wikiText = "{|\n! Header 1\n! Header 2\n|-\n| Cell 1\n| Cell 2\n|}";
        string html = _parser.ToHtml(wikiText);

        Assert.Contains("<table class=\"table table-bordered table-striped\">", html);
        Assert.Contains("<th> Header 1</th>", html);
        Assert.Contains("<th> Header 2</th>", html);
        Assert.Contains("<tr>", html);
        Assert.Contains("<td> Cell 1</td>", html);
        Assert.Contains("<td> Cell 2</td>", html);
        Assert.Contains("</table>", html);
    }

    [Fact]
    public void TestTableWithAttributes()
    {
        string wikiText = "{| class=\"wikitable\"\n! Header\n|-\n| style=\"color:red\" | Cell\n|}";
        string html = _parser.ToHtml(wikiText);
        
        System.Console.WriteLine("DEBUG TABLE ATTR: " + html);

        Assert.Contains("class=\"table table-bordered table-striped wikitable\"", html);
        Assert.Contains("<td style=\"color:red\"> Cell", html);
    }

    [Fact]
    public void TestTableShorthand()
    {
        string wikiText = "{| class=\"wikitable\"\n! Anbieter !! Modell-Zugang !! Limit (Rate Limits) !! Besonderheit\n|-\n| '''Google Gemini (AI Studio)''' || Gemini 2.5 Pro / Flash || bis zu 15 RPM / 1M Kontext || Bestes Gratis-Paket, kein CC nötig.\n|}";
        string html = _parser.ToHtml(wikiText);
        
        System.Console.WriteLine("DEBUG SHORTHAND: " + html);

        Assert.Contains("<th> Anbieter </th>", html);
        Assert.Contains("<th> Modell-Zugang </th>", html);
        Assert.Contains("<b data-mw=\"bold\">Google Gemini (AI Studio)</b>", html);
        Assert.Contains("<td> Gemini 2.5 Pro / Flash </td>", html);
    }
}
