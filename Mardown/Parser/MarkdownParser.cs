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
