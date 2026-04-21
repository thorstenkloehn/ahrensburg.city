/*
 * ahrensburg.city (MeinCMS) - A lightweight CMS with Wiki functionality and multi-tenancy.
 * Copyright (C) 2026 Thorsten
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Text;
using System.Web;
using Mardown.Models;

namespace Mardown.Parser;

public class MarkdownASTSerializer
{
    public string ToHtml(MarkdownNode node)
    {
        var sb = new StringBuilder();
        Serialize(node, sb);
        return sb.ToString();
    }

    private void Serialize(MarkdownNode node, StringBuilder sb)
    {
        switch (node)
        {
            case RootNode root:
                foreach (var child in root.Children) Serialize(child, sb);
                break;

            case HeadingNode h:
                sb.Append($"<h{h.Level}>");
                foreach (var child in h.Children) Serialize(child, sb);
                sb.Append($"</h{h.Level}>\n");
                break;

            case BoldNode b:
                sb.Append("<strong>");
                foreach (var child in b.Children) Serialize(child, sb);
                sb.Append("</strong>");
                break;

            case ItalicNode i:
                sb.Append("<em>");
                foreach (var child in i.Children) Serialize(child, sb);
                sb.Append("</em>");
                break;

            case LinkNode l:
                sb.Append($"<a href=\"{HttpUtility.HtmlAttributeEncode(l.Url)}\">{HttpUtility.HtmlEncode(l.Label)}</a>");
                break;

            case TemplateNode t:
                sb.Append($"<div class=\"markdown-template\" data-name=\"{HttpUtility.HtmlAttributeEncode(t.Name)}\">");
                sb.Append(HttpUtility.HtmlEncode(string.Join(", ", t.Parameters)));
                sb.Append("</div>");
                break;

            case ListNode list:
                var tag = list.IsOrdered ? "ol" : "ul";
                sb.Append($"<{tag}>\n");
                foreach (var child in list.Children) Serialize(child, sb);
                sb.Append($"</{tag}>\n");
                break;

            case ListItemNode item:
                sb.Append("<li>");
                foreach (var child in item.Children) Serialize(child, sb);
                sb.Append("</li>\n");
                break;

            case TextNode text:
                sb.Append(HttpUtility.HtmlEncode(text.Text).Replace("\n", "<br />\n"));
                break;

            case ParagraphNode p:
                sb.Append("<p>");
                foreach (var child in p.Children) Serialize(child, sb);
                sb.Append("</p>\n");
                break;

            case CategoryNode:
                // Kategorien werden im HTML nicht angezeigt (gelöscht)
                break;

            case TableNode table:
                sb.Append("<table>\n");
                foreach (var child in table.Children) Serialize(child, sb);
                sb.Append("</table>\n");
                break;

            case TableRowNode row:
                sb.Append("<tr>\n");
                foreach (var child in row.Children)
                {
                    if (row.IsHeader && child is TableCellNode cell)
                    {
                        sb.Append("<th>");
                        foreach (var c in cell.Children) Serialize(c, sb);
                        sb.Append("</th>");
                    }
                    else
                    {
                        Serialize(child, sb);
                    }
                }
                sb.Append("</tr>\n");
                break;

            case TableCellNode cell:
                sb.Append("<td>");
                foreach (var child in cell.Children) Serialize(child, sb);
                sb.Append("</td>");
                break;

            case CodeInlineNode ci:
                sb.Append("<code>");
                sb.Append(HttpUtility.HtmlEncode(ci.Code));
                sb.Append("</code>");
                break;

            case CodeBlockNode cb:
                sb.Append("<pre><code>");
                sb.Append(HttpUtility.HtmlEncode(cb.Code));
                sb.Append("</code></pre>\n");
                break;
        }
    }
}
