using System.Collections.Generic;
using System.Linq;
using mvc.Parser.Models;

namespace mvc.Parser;

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
        for (int i = 0; i < tokenList.Count; i++)
        {
            var token = tokenList[i];

            switch (token.Type)
            {
                case TokenType.Text:
                    stack.Peek().Children.Add(new TextNode { Text = token.Value });
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
                    break;
                
                case TokenType.HeadingStart:
                    if (stack.Peek() is HeadingNode)
                    {
                        stack.Pop();
                    }
                    else
                    {
                        var level = token.Value.Trim().Length;
                        var heading = new HeadingNode { Level = level };
                        stack.Peek().Children.Add(heading);
                        stack.Push(heading);
                    }
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
                    break;

                case TokenType.NewLine:
                    // Close list item and other inline nodes on newline
                    while (stack.Count > 1 && (stack.Peek() is ListItemNode || stack.Peek() is BoldNode || stack.Peek() is ItalicNode || stack.Peek() is HeadingNode))
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
                    break;

                // Simple case for others
                default:
                    stack.Peek().Children.Add(new TextNode { Text = token.Value });
                    break;
            }
        }

        return root;
    }
}
