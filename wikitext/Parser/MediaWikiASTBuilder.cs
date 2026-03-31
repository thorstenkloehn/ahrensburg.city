using System.Collections.Generic;
using System.Linq;
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
        for (int i = 0; i < tokenList.Count; i++)
        {
            var token = tokenList[i];

            switch (token.Type)
            {
                case TokenType.Text:
                    stack.Peek().Children.Add(new TextNode { Text = token.Value });
                    startOfLine = false;
                    break;

                case TokenType.BoldStart:
                    if (stack.Peek() is BoldNode)
                    {
                        stack.Pop();
                    }
                    else
                    {
                        var bold = new BoldNode();
                        stack.Peek().Children.Add(bold);
                        stack.Push(bold);
                    }
                    startOfLine = false;
                    break;

                case TokenType.ItalicStart:
                    if (stack.Peek() is ItalicNode)
                    {
                        stack.Pop();
                    }
                    else
                    {
                        var italic = new ItalicNode();
                        stack.Peek().Children.Add(italic);
                        stack.Push(italic);
                    }
                    startOfLine = false;
                    break;
                
                case TokenType.HeadingStart:
                    if (stack.Peek() is HeadingNode)
                    {
                        var heading = (HeadingNode)stack.Peek();
                        // Trim content of heading
                        if (heading.Children.Count > 0)
                        {
                            if (heading.Children[0] is TextNode firstText) firstText.Text = firstText.Text.TrimStart();
                            if (heading.Children.Last() is TextNode lastText) lastText.Text = lastText.Text.TrimEnd();
                            
                            // Remove empty text nodes after trimming
                            heading.Children.RemoveAll(c => c is TextNode tn && string.IsNullOrEmpty(tn.Text));
                        }
                        // Match - pop heading
                        stack.Pop();
                    }
                    else if (startOfLine)
                    {
                        var level = token.Value.Trim().Length;
                        var heading = new HeadingNode { Level = level };
                        stack.Peek().Children.Add(heading);
                        stack.Push(heading);
                    }
                    else
                    {
                        // Unmatched heading marker in the middle of a line -> treat as text
                        // BUT ONLY if we are NOT inside a heading already (trailing marker case)
                        if (!(stack.Peek() is HeadingNode))
                        {
                            stack.Peek().Children.Add(new TextNode { Text = token.Value });
                        }
                    }
                    startOfLine = false;
                    break;

                case TokenType.LinkStart:
                    // Simple link parsing [[Target|Display]]
                    int j = i + 1;
                    string content = "";
                    while (j < tokenList.Count && tokenList[j].Type != TokenType.LinkEnd)
                    {
                        content += tokenList[j].Value;
                        j++;
                    }
                    var parts = content.Split('|');
                    stack.Peek().Children.Add(new LinkNode 
                    { 
                        Target = parts[0], 
                        Display = parts.Length > 1 ? parts[1] : parts[0] 
                    });
                    if (j < tokenList.Count) i = j; // Skip to LinkEnd if found
                    else i = tokenList.Count; // Else skip to end
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
                    // External link is [URL Description] - split by first space
                    var firstSpace = extContent.IndexOf(' ');
                    if (firstSpace != -1)
                    {
                        stack.Peek().Children.Add(new LinkNode 
                        { 
                            Target = extContent.Substring(0, firstSpace), 
                            Display = extContent.Substring(firstSpace + 1).Trim(),
                            IsExternal = true
                        });
                    }
                    else
                    {
                        stack.Peek().Children.Add(new LinkNode 
                        { 
                            Target = extContent, 
                            Display = extContent,
                            IsExternal = true
                        });
                    }
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
                    stack.Peek().Children.Add(new CategoryNode 
                    { 
                        CategoryName = catParts[0].Trim(),
                        SortKey = catParts.Length > 1 ? catParts[1].Trim() : null
                    });
                    if (k < tokenList.Count) i = k; // Skip to LinkEnd if found
                    else i = tokenList.Count; // Else skip to end
                    startOfLine = false;
                    break;

                case TokenType.TableStart:
                    var tableNode = new TableNode();
                    // Peek next token for attributes
                    if (i + 1 < tokenList.Count && tokenList[i+1].Type == TokenType.Text)
                    {
                        var attrToken = tokenList[i+1];
                        if (!attrToken.Value.Contains("\n"))
                        {
                            tableNode.Attributes = attrToken.Value.Trim();
                            i++; // Skip attribute token
                        }
                    }
                    stack.Peek().Children.Add(tableNode);
                    stack.Push(tableNode);
                    startOfLine = false;
                    break;

                case TokenType.TableEnd:
                    while (stack.Count > 1 && !(stack.Peek() is TableNode))
                    {
                        stack.Pop();
                    }
                    if (stack.Count > 1 && stack.Peek() is TableNode)
                    {
                        stack.Pop();
                    }
                    startOfLine = false;
                    break;

                case TokenType.TableRow:
                    while (stack.Count > 1 && !(stack.Peek() is TableNode))
                    {
                        stack.Pop();
                    }
                    if (stack.Peek() is TableNode table)
                    {
                        // Check if the last child of the table is an empty row
                        if (table.Children.LastOrDefault() is TableRowNode lastRow && lastRow.Children.Count == 0)
                        {
                            // Reuse empty row
                            stack.Push(lastRow);
                        }
                        else
                        {
                            var rowNode = new TableRowNode();
                            // Peek next token for attributes
                            if (i + 1 < tokenList.Count && tokenList[i+1].Type == TokenType.Text)
                            {
                                var attrToken = tokenList[i+1];
                                if (!attrToken.Value.Contains("\n"))
                                {
                                    rowNode.Attributes = attrToken.Value.Trim();
                                    i++; // Skip attribute token
                                }
                            }
                            table.Children.Add(rowNode);
                            stack.Push(rowNode);
                        }
                    }
                    startOfLine = false;
                    break;

                case TokenType.TableCell:
                case TokenType.TableHeader:
                    while (stack.Count > 1 && !(stack.Peek() is TableRowNode || stack.Peek() is TableNode))
                    {
                        stack.Pop();
                    }
                    
                    if (stack.Peek() is TableNode currentTable)
                    {
                        var autoRow = new TableRowNode();
                        currentTable.Children.Add(autoRow);
                        stack.Push(autoRow);
                    }

                    if (stack.Peek() is TableRowNode row)
                    {
                        // If we are already in a cell, pop it first (shorthand !! or || case)
                        if (stack.Peek() is TableCellNode)
                        {
                            stack.Pop();
                        }

                        var cellNode = new TableCellNode { IsHeader = token.Type == TokenType.TableHeader };
                        
                        // In MediaWiki, cell attributes are separated by |
                        // e.g. | style="color:red" | Content
                        // ONLY for single | (TableCell) or single ! (TableHeader)
                        // If the token value is "||" or "!!", it's shorthand and doesn't have attributes prefix.
                        if (token.Value.Length == 1 && i + 2 < tokenList.Count && tokenList[i+1].Type == TokenType.Text && tokenList[i+2].Type == TokenType.TableCell)
                        {
                            cellNode.Attributes = tokenList[i+1].Value.Trim();
                            i += 2; // Skip attributes and the second |
                        }

                        row.Children.Add(cellNode);
                        stack.Push(cellNode);
                    }
                    startOfLine = false;
                    break;

                case TokenType.BulletList:
                case TokenType.NumberedList:
                    var listType = token.Type == TokenType.BulletList ? ListType.Unordered : ListType.Ordered;
                    
                    // If we're not in a list or the type has changed, start a new list
                    if (!(stack.Peek() is ListNode existingList && existingList.Type == listType))
                    {
                        while (stack.Count > 1 && (stack.Peek() is BoldNode || stack.Peek() is ItalicNode)) stack.Pop();
                        
                        var newList = new ListNode { Type = listType };
                        stack.Peek().Children.Add(newList);
                        stack.Push(newList);
                    }
                    
                    var listItem = new ListItemNode();
                    stack.Peek().Children.Add(listItem);
                    stack.Push(listItem);
                    startOfLine = false;
                    break;

                case TokenType.NewLine:
                    // Close list item and other inline nodes on newline
                    while (stack.Count > 1 && (stack.Peek() is ListItemNode || stack.Peek() is BoldNode || stack.Peek() is ItalicNode || stack.Peek() is HeadingNode || stack.Peek() is TableCellNode || stack.Peek() is TableRowNode))
                    {
                        stack.Pop();
                    }
                    
                    // If the next token isn't another list item of the same type, close the list too
                    if (i + 1 < tokenList.Count)
                    {
                        var nextType = tokenList[i+1].Type;
                        if (stack.Peek() is ListNode currentList)
                        {
                            bool nextMatches = (currentList.Type == ListType.Unordered && nextType == TokenType.BulletList) ||
                                              (currentList.Type == ListType.Ordered && nextType == TokenType.NumberedList);
                            if (!nextMatches) stack.Pop();
                        }
                    }
                    else if (stack.Peek() is ListNode)
                    {
                        stack.Pop();
                    }

                    stack.Peek().Children.Add(new TextNode { Text = "\n" });
                    startOfLine = true;
                    break;

                case TokenType.CodeStart:
                    var codeNode = new CodeNode();
                    stack.Peek().Children.Add(codeNode);
                    stack.Push(codeNode);
                    startOfLine = false;
                    break;

                case TokenType.CodeEnd:
                    if (stack.Peek() is CodeNode)
                    {
                        stack.Pop();
                    }
                    startOfLine = false;
                    break;

                // Simple case for others
                default:
                    stack.Peek().Children.Add(new TextNode { Text = token.Value });
                    startOfLine = false;
                    break;
            }
        }

        return root;
    }
}
