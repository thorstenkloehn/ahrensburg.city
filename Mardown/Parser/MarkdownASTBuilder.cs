using System.Collections.Generic;
using Mardown.Models;
using System.Linq;

namespace Mardown.Parser;

public class MarkdownASTBuilder
{
    private MarkdownTokenizer? _tokenizer;

    public RootNode Build(List<MarkdownToken> tokens, MarkdownTokenizer? tokenizer = null)
    {
        _tokenizer = tokenizer;
        var root = new RootNode();
        ProcessTokens(tokens, root);
        return root;
    }

    private void ProcessTokens(List<MarkdownToken> tokens, MarkdownNode parent)
    {
        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];

            if (parent is RootNode && IsInline(token.Type))
            {
                var p = new ParagraphNode();
                while (i < tokens.Count)
                {
                    if (tokens[i].Type == MarkdownTokenType.Newline)
                    {
                        if (i + 1 < tokens.Count && tokens[i + 1].Type == MarkdownTokenType.Newline)
                        {
                            i++; // consume first newline
                            break; 
                        }
                        
                        // Only add newline if there's more inline content coming
                        bool hasMoreInline = false;
                        for (int k = i + 1; k < tokens.Count; k++)
                        {
                            if (IsInline(tokens[k].Type)) { hasMoreInline = true; break; }
                            if (tokens[k].Type != MarkdownTokenType.Newline) break;
                        }

                        if (hasMoreInline)
                        {
                            p.Children.Add(new TextNode { Text = "\n" });
                        }
                    }
                    else if (IsInline(tokens[i].Type))
                    {
                        ProcessSingleToken(tokens[i], p);
                    }
                    else
                    {
                        break;
                    }
                    i++;
                }
                
                if (p.Children.Count > 0)
                {
                    parent.Children.Add(p);
                }

                if (i < tokens.Count) i--; 
                continue;
            }

            if (token.Type == MarkdownTokenType.Template)
            {
                parent.Children.Add(new TemplateNode 
                { 
                    Name = token.Value, 
                    Parameters = token.Parameters 
                });
                continue;
            }

            if (token.Type == MarkdownTokenType.List)
            {
                var list = new ListNode { IsOrdered = false };
                while (i < tokens.Count && tokens[i].Type == MarkdownTokenType.List)
                {
                    var item = new ListItemNode();
                    if (_tokenizer != null)
                    {
                        var subTokens = new List<MarkdownToken>();
                        _tokenizer.TokenizeInline(tokens[i].Value, subTokens);
                        ProcessTokens(subTokens, item);
                    }
                    else
                    {
                        item.Children.Add(new TextNode { Text = tokens[i].Value });
                    }
                    list.Children.Add(item);
                    i++;
                }
                i--;
                parent.Children.Add(list);
                continue;
            }

            if (token.Type == MarkdownTokenType.CodeBlock)
            {
                parent.Children.Add(new CodeBlockNode { Code = token.Value });
                continue;
            }

            if (token.Type == MarkdownTokenType.TableRow)
            {
                var table = new TableNode();
                var rowTokens = new List<MarkdownToken>();
                bool hasDivider = false;

                int j = i;
                while (j < tokens.Count)
                {
                    if (tokens[j].Type == MarkdownTokenType.TableRow || tokens[j].Type == MarkdownTokenType.TableDivider)
                    {
                        if (tokens[j].Type == MarkdownTokenType.TableDivider) hasDivider = true;
                        rowTokens.Add(tokens[j]);
                    }
                    else if (tokens[j].Type == MarkdownTokenType.Newline)
                    {
                        // Check if it's a double newline (end of table)
                        if (j + 1 < tokens.Count && tokens[j + 1].Type == MarkdownTokenType.Newline)
                        {
                            break;
                        }
                        // Single newline within table - check if more table rows follow
                        bool moreRows = false;
                        for (int k = j + 1; k < tokens.Count; k++)
                        {
                            if (tokens[k].Type == MarkdownTokenType.TableRow) { moreRows = true; break; }
                            if (tokens[k].Type != MarkdownTokenType.Newline) break;
                        }
                        if (!moreRows) break;
                        
                        // Ignore single newline inside table
                    }
                    else
                    {
                        break;
                    }
                    j++;
                }

                for (int k = 0; k < rowTokens.Count; k++)
                {
                    if (rowTokens[k].Type == MarkdownTokenType.TableDivider) continue;

                    var rowNode = new TableRowNode();
                    if (k == 0 && hasDivider && rowTokens.Count > 1 && rowTokens[1].Type == MarkdownTokenType.TableDivider)
                    {
                        rowNode.IsHeader = true;
                    }

                    var cells = rowTokens[k].Value.Split('|', System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var cellText in cells)
                    {
                        var cellNode = new TableCellNode();
                        if (_tokenizer != null)
                        {
                            var cellTokens = new List<MarkdownToken>();
                            _tokenizer.TokenizeInline(cellText.Trim(), cellTokens);
                            ProcessTokens(cellTokens, cellNode);
                        }
                        else
                        {
                            cellNode.Children.Add(new TextNode { Text = cellText.Trim() });
                        }
                        rowNode.Children.Add(cellNode);
                    }
                    table.Children.Add(rowNode);
                }

                parent.Children.Add(table);
                i = j - 1;
                continue;
            }

            ProcessSingleToken(token, parent);
        }
    }

    private void ProcessSingleToken(MarkdownToken token, MarkdownNode parent)
    {
        switch (token.Type)
        {
            case MarkdownTokenType.Heading:
                var heading = new HeadingNode { Level = token.Level };
                if (_tokenizer != null)
                {
                    var subTokens = new List<MarkdownToken>();
                    _tokenizer.TokenizeInline(token.Value, subTokens);
                    ProcessTokens(subTokens, heading);
                }
                else
                {
                    heading.Children.Add(new TextNode { Text = token.Value });
                }
                parent.Children.Add(heading);
                break;

            case MarkdownTokenType.Bold:
                var bold = new BoldNode();
                if (_tokenizer != null)
                {
                    var subTokens = new List<MarkdownToken>();
                    _tokenizer.TokenizeInline(token.Value, subTokens);
                    ProcessTokens(subTokens, bold);
                }
                else
                {
                    bold.Children.Add(new TextNode { Text = token.Value });
                }
                parent.Children.Add(bold);
                break;

            case MarkdownTokenType.Italic:
                var italic = new ItalicNode();
                if (_tokenizer != null)
                {
                    var subTokens = new List<MarkdownToken>();
                    _tokenizer.TokenizeInline(token.Value, subTokens);
                    ProcessTokens(subTokens, italic);
                }
                else
                {
                    italic.Children.Add(new TextNode { Text = token.Value });
                }
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
                parent.Children.Add(new TemplateNode 
                { 
                    Name = token.Value, 
                    Parameters = token.Parameters 
                });
                break;

            case MarkdownTokenType.CodeInline:
                parent.Children.Add(new CodeInlineNode { Code = token.Value });
                break;

            case MarkdownTokenType.Text:
                parent.Children.Add(new TextNode { Text = token.Value });
                break;

            case MarkdownTokenType.Category:
                parent.Children.Add(new CategoryNode { Name = token.Value });
                break;
        }
    }

    private bool IsInline(MarkdownTokenType type)
    {
        return type == MarkdownTokenType.Text || 
               type == MarkdownTokenType.Bold || 
               type == MarkdownTokenType.Italic || 
               type == MarkdownTokenType.Link || 
               type == MarkdownTokenType.Category ||
               type == MarkdownTokenType.CodeInline;
    }
}
