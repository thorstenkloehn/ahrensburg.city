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
