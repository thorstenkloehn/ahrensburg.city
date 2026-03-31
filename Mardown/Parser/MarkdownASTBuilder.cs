using System.Collections.Generic;
using Mardown.Models;

namespace Mardown.Parser;

public class MarkdownASTBuilder
{
    public RootNode Build(List<MarkdownToken> tokens)
    {
        var root = new RootNode();
        ProcessTokens(tokens, root);
        return root;
    }

    private void ProcessTokens(List<MarkdownToken> tokens, MarkdownNode parent)
    {
        foreach (var token in tokens)
        {
            switch (token.Type)
            {
                case MarkdownTokenType.Heading:
                    parent.Children.Add(new HeadingNode 
                    { 
                        Level = token.Level, 
                        Children = new List<MarkdownNode> { new TextNode { Text = token.Value } } 
                    });
                    break;

                case MarkdownTokenType.Bold:
                    var bold = new BoldNode();
                    bold.Children.Add(new TextNode { Text = token.Value });
                    parent.Children.Add(bold);
                    break;

                case MarkdownTokenType.Italic:
                    var italic = new ItalicNode();
                    italic.Children.Add(new TextNode { Text = token.Value });
                    parent.Children.Add(italic);
                    break;

                case MarkdownTokenType.Link:
                    parent.Children.Add(new LinkNode 
                    { 
                        Url = token.Value, 
                        Label = token.Parameters.Count > 0 ? token.Parameters[0] : token.Value 
                    });
                    break;

                case MarkdownTokenType.Template:
                    var template = new TemplateNode 
                    { 
                        Name = token.Value, 
                        Parameters = token.Parameters 
                    };
                    // Recursive call example for templates (if they contain markdown)
                    // In a real system, you'd look up the template content and parse it here.
                    parent.Children.Add(template);
                    break;

                case MarkdownTokenType.List:
                    // Simple list handling
                    var list = new ListNode { IsOrdered = false };
                    var item = new ListItemNode();
                    item.Children.Add(new TextNode { Text = token.Value });
                    list.Children.Add(item);
                    parent.Children.Add(list);
                    break;

                case MarkdownTokenType.Text:
                    parent.Children.Add(new TextNode { Text = token.Value });
                    break;

                case MarkdownTokenType.Category:
                    parent.Children.Add(new CategoryNode { Name = token.Value });
                    break;

                case MarkdownTokenType.Newline:
                    // Could be a paragraph break if there are two
                    break;
            }
        }
    }
}
