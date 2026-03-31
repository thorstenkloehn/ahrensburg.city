using Wikitext.Models;
using Wikitext.Parser;

namespace Wikitext;

/// <summary>
/// Provides high-level methods to generate WikiText from an Abstract Syntax Tree (AST).
/// </summary>
public static class WikiTextGenerator
{
    private static readonly IMediaWikiASTSerializer _serializer = new MediaWikiASTSerializer();

    /// <summary>
    /// Generates WikiText from the given WikiNode (AST).
    /// </summary>
    /// <param name="node">The root node or any node of the AST.</param>
    /// <returns>The generated WikiText string.</returns>
    public static string Generate(WikiNode node)
    {
        return _serializer.ToWikiText(node);
    }

    /// <summary>
    /// Generates HTML from the given WikiNode (AST).
    /// </summary>
    /// <param name="node">The root node or any node of the AST.</param>
    /// <returns>The generated HTML string.</returns>
    public static string GenerateHtml(WikiNode node)
    {
        return _serializer.ToHtml(node);
    }
}
