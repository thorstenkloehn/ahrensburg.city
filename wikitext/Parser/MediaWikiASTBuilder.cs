using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Wikitext.Models;

namespace Wikitext.Parser;

public interface IMediaWikiASTBuilder
{
    RootNode Build(IEnumerable<Token> tokens);
}

public class MediaWikiASTBuilder : IMediaWikiASTBuilder
{
    public RootNode Build(IEnumerable<Token> tokens)
    {
        var root = new RootNode();
        var stack = new Stack<WikiNode>();
        stack.Push(root);

        var tokenList = tokens.ToList();
        bool startOfLine = true;
        int consecutiveNewLines = 0;

        void CloseInlineAndBlockIfNeeded(bool forceCloseParagraph = false)
        {
            // Close inline formatting nodes (Bold, Italic, Code)
            while (stack.Count > 1 && (stack.Peek() is BoldNode || stack.Peek() is ItalicNode || stack.Peek() is CodeNode))
            {
                stack.Pop();
            }

            // Close Heading and ListItem on NewLine or Block Element
            if (stack.Count > 1 && (stack.Peek() is HeadingNode || stack.Peek() is ListItemNode))
            {
                stack.Pop();
            }

            if (forceCloseParagraph)
            {
                // Close Paragraph too
                if (stack.Count > 1 && stack.Peek() is ParagraphNode)
                {
                    stack.Pop();
                }
            }
        }

        WikiNode EnsureContainer(bool isBlock)
        {
            if (isBlock)
            {
                // Close existing paragraph if we start a block
                if (stack.Peek() is ParagraphNode) stack.Pop();
                // Ensure we are in RootNode or something that can contain blocks
                while (stack.Count > 1 && (stack.Peek() is ParagraphNode || stack.Peek() is BoldNode || stack.Peek() is ItalicNode || stack.Peek() is HeadingNode || stack.Peek() is ListItemNode))
                {
                    stack.Pop();
                }
                return stack.Peek();
            }
            else
            {
                // Inline content
                if (stack.Peek() is RootNode)
                {
                    var p = new ParagraphNode();
                    stack.Peek().Children.Add(p);
                    stack.Push(p);
                }
                return stack.Peek();
            }
        }

        for (int i = 0; i < tokenList.Count; i++)
        {
            var token = tokenList[i];

            if (token.Type == TokenType.NewLine)
            {
                consecutiveNewLines++;
                
                // On NewLine, close short-lived inline nodes (Bold, Italic, Heading, ListItem)
                // BUT don't necessarily close the paragraph unless it's a double newline
                
                if (consecutiveNewLines >= 2)
                {
                    // Double NewLine -> Close Paragraph
                    CloseInlineAndBlockIfNeeded(forceCloseParagraph: true);
                }
                else
                {
                    // Single NewLine -> just close current ListItem/Heading
                    CloseInlineAndBlockIfNeeded(forceCloseParagraph: false);
                    
                    // If we are in a paragraph, we might want to keep the \n as a space
                    // or as a text node for the serializer to decide.
                    if (stack.Peek() is ParagraphNode)
                    {
                        stack.Peek().Children.Add(new TextNode { Text = "\n" });
                    }
                    else if (stack.Peek() is RootNode)
                    {
                        // Ignore structural newlines at the root
                    }
                }

                startOfLine = true;
                continue;
            }

            consecutiveNewLines = 0;

            switch (token.Type)
            {
                case TokenType.Text:
                    EnsureContainer(isBlock: false).Children.Add(new TextNode { Text = token.Value });
                    startOfLine = false;
                    break;

                case TokenType.BoldStart:
                    var boldContainer = EnsureContainer(isBlock: false);
                    if (stack.Peek() is BoldNode)
                    {
                        stack.Pop();
                    }
                    else
                    {
                        var bold = new BoldNode();
                        boldContainer.Children.Add(bold);
                        stack.Push(bold);
                    }
                    startOfLine = false;
                    break;

                case TokenType.ItalicStart:
                    var italicContainer = EnsureContainer(isBlock: false);
                    if (stack.Peek() is ItalicNode)
                    {
                        stack.Pop();
                    }
                    else
                    {
                        var italic = new ItalicNode();
                        italicContainer.Children.Add(italic);
                        stack.Push(italic);
                    }
                    startOfLine = false;
                    break;
                
                case TokenType.HeadingStart:
                    if (startOfLine)
                    {
                        var rootForBlock = EnsureContainer(isBlock: true);
                        var level = token.Value.Trim().Length;
                        var heading = new HeadingNode { Level = level };
                        rootForBlock.Children.Add(heading);
                        stack.Push(heading);
                    }
                    else
                    {
                        // Trailing heading marker
                        if (stack.Peek() is HeadingNode)
                        {
                            var h = (HeadingNode)stack.Peek();
                            // Trim content of heading
                            if (h.Children.Count > 0)
                            {
                                if (h.Children[0] is TextNode firstText) firstText.Text = firstText.Text.TrimStart();
                                if (h.Children.Last() is TextNode lastText) lastText.Text = lastText.Text.TrimEnd();
                                h.Children.RemoveAll(c => c is TextNode tn && string.IsNullOrEmpty(tn.Text));
                            }
                            stack.Pop();
                        }
                        else
                        {
                            EnsureContainer(isBlock: false).Children.Add(new TextNode { Text = token.Value });
                        }
                    }
                    startOfLine = false;
                    break;

                case TokenType.LinkStart:
                    int j = i + 1;
                    string content = "";
                    while (j < tokenList.Count && tokenList[j].Type != TokenType.LinkEnd)
                    {
                        content += tokenList[j].Value;
                        j++;
                    }
                    var parts = content.Split('|');
                    EnsureContainer(isBlock: false).Children.Add(new LinkNode 
                    { 
                        Target = parts[0], 
                        Display = parts.Length > 1 ? parts[1] : parts[0] 
                    });
                    if (j < tokenList.Count) i = j;
                    else i = tokenList.Count;
                    startOfLine = false;
                    break;

                case TokenType.ExternalLinkStart:
                    int l = i + 1;
                    string extContent = "";
                    while (l < tokenList.Count && tokenList[l].Type != TokenType.ExternalLinkEnd)
                    {
                        extContent += tokenList[l].Value;
                        l++;
                    }
                    var firstSpace = extContent.IndexOf(' ');
                    var eLink = new LinkNode { IsExternal = true };
                    if (firstSpace != -1)
                    {
                        eLink.Target = extContent.Substring(0, firstSpace);
                        eLink.Display = extContent.Substring(firstSpace + 1).Trim();
                    }
                    else
                    {
                        eLink.Target = extContent;
                        eLink.Display = extContent;
                    }
                    EnsureContainer(isBlock: false).Children.Add(eLink);
                    if (l < tokenList.Count) i = l;
                    else i = tokenList.Count;
                    startOfLine = false;
                    break;

                case TokenType.CategoryStart:
                    int k = i + 1;
                    string catContent = "";
                    while (k < tokenList.Count && tokenList[k].Type != TokenType.LinkEnd)
                    {
                        catContent += tokenList[k].Value;
                        k++;
                    }
                    var catParts = catContent.Split('|');
                    root.Children.Add(new CategoryNode 
                    { 
                        CategoryName = catParts[0].Trim(),
                        SortKey = catParts.Length > 1 ? catParts[1].Trim() : null
                    });
                    if (k < tokenList.Count) i = k;
                    else i = tokenList.Count;
                    startOfLine = false;
                    break;

                case TokenType.TableStart:
                    var tableNode = new TableNode();
                    if (i + 1 < tokenList.Count && tokenList[i+1].Type == TokenType.Text)
                    {
                        var attrToken = tokenList[i+1];
                        if (!attrToken.Value.Contains("\n"))
                        {
                            tableNode.Attributes = attrToken.Value.Trim();
                            i++;
                        }
                    }
                    EnsureContainer(isBlock: true).Children.Add(tableNode);
                    stack.Push(tableNode);
                    startOfLine = false;
                    break;

                case TokenType.TableEnd:
                    while (stack.Count > 1 && !(stack.Peek() is TableNode)) stack.Pop();
                    if (stack.Count > 1 && stack.Peek() is TableNode) stack.Pop();
                    startOfLine = false;
                    break;

                case TokenType.TableRow:
                    while (stack.Count > 1 && !(stack.Peek() is TableNode)) stack.Pop();
                    if (stack.Peek() is TableNode table)
                    {
                        if (!(table.Children.LastOrDefault() is TableRowNode lastRow && lastRow.Children.Count == 0))
                        {
                            var rowNode = new TableRowNode();
                            if (i + 1 < tokenList.Count && tokenList[i+1].Type == TokenType.Text)
                            {
                                var attrToken = tokenList[i+1];
                                if (!attrToken.Value.Contains("\n"))
                                {
                                    rowNode.Attributes = attrToken.Value.Trim();
                                    i++;
                                }
                            }
                            table.Children.Add(rowNode);
                            stack.Push(rowNode);
                        }
                        else
                        {
                            stack.Push(lastRow);
                        }
                    }
                    startOfLine = false;
                    break;

                case TokenType.TableCell:
                case TokenType.TableHeader:
                    while (stack.Count > 1 && !(stack.Peek() is TableRowNode || stack.Peek() is TableNode)) stack.Pop();
                    if (stack.Peek() is TableNode t)
                    {
                        var autoRow = new TableRowNode();
                        t.Children.Add(autoRow);
                        stack.Push(autoRow);
                    }
                    if (stack.Peek() is TableRowNode tr)
                    {
                        if (stack.Peek() is TableCellNode) stack.Pop();
                        var cellNode = new TableCellNode { IsHeader = token.Type == TokenType.TableHeader };
                        if (token.Value.Length == 1 && i + 2 < tokenList.Count && tokenList[i+1].Type == TokenType.Text && tokenList[i+2].Type == TokenType.TableCell)
                        {
                            cellNode.Attributes = tokenList[i+1].Value.Trim();
                            i += 2;
                        }
                        tr.Children.Add(cellNode);
                        stack.Push(cellNode);
                    }
                    startOfLine = false;
                    break;

                case TokenType.BulletList:
                case TokenType.NumberedList:
                    var lType = token.Type == TokenType.BulletList ? ListType.Unordered : ListType.Ordered;
                    
                    if (stack.Peek() is ListNode currentList && currentList.Type == lType)
                    {
                        // Already in the right list
                    }
                    else
                    {
                        var listRoot = EnsureContainer(isBlock: true);
                        if (!(listRoot.Children.LastOrDefault() is ListNode existingList && existingList.Type == lType))
                        {
                            existingList = new ListNode { Type = lType };
                            listRoot.Children.Add(existingList);
                        }
                        
                        while (stack.Count > 1 && stack.Peek() != listRoot) stack.Pop();
                        stack.Push(existingList);
                    }
                    
                    var li = new ListItemNode();
                    stack.Peek().Children.Add(li);
                    stack.Push(li);
                    startOfLine = false;
                    break;

                case TokenType.TemplateStart:
                    int m = i + 1;
                    string tempContent = "";
                    while (m < tokenList.Count && tokenList[m].Type != TokenType.TemplateEnd)
                    {
                        tempContent += tokenList[m].Value;
                        m++;
                    }
                    var tParts = tempContent.Split('|');
                    var templateNode = new TemplateNode { TemplateName = tParts[0].Trim() };
                    for (int n = 1; n < tParts.Length; n++)
                    {
                        var pMatch = Regex.Match(tParts[n], @"^(.*?)=(.*)$");
                        if (pMatch.Success) templateNode.Parameters[pMatch.Groups[1].Value.Trim()] = pMatch.Groups[2].Value.Trim();
                        else templateNode.Parameters[n.ToString()] = tParts[n].Trim();
                    }
                    EnsureContainer(isBlock: false).Children.Add(templateNode);
                    if (m < tokenList.Count) i = m;
                    else i = tokenList.Count;
                    startOfLine = false;
                    break;

                case TokenType.CodeStart:
                    var codeNode = new CodeNode();
                    EnsureContainer(isBlock: false).Children.Add(codeNode);
                    stack.Push(codeNode);
                    startOfLine = false;
                    break;

                case TokenType.CodeEnd:
                    if (stack.Peek() is CodeNode) stack.Pop();
                    startOfLine = false;
                    break;

                case TokenType.TagStart:
                case TokenType.TagEnd:
                    EnsureContainer(isBlock: false).Children.Add(new HtmlTagNode { Tag = token.Value });
                    startOfLine = false;
                    break;

                default:
                    EnsureContainer(isBlock: false).Children.Add(new TextNode { Text = token.Value });
                    startOfLine = false;
                    break;
            }
        }

        return root;
    }
}
