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
using Wikitext.Models;

namespace Wikitext.Parser;

public interface IMediaWikiASTSerializer
{
    string ToHtml(WikiNode node);
    string ToWikiText(WikiNode node);
}

public class MediaWikiASTSerializer : IMediaWikiASTSerializer
{
    public string ToHtml(WikiNode node)
    {
        return SerializeNodeToHtml(node);
    }

    private string SerializeNodeToHtml(WikiNode node)
    {
        var sb = new StringBuilder();
        switch (node)
        {
            case TextNode text:
                sb.Append(System.Net.WebUtility.HtmlEncode(text.Text).Replace("\n", "<br />\n"));
                break;
            case BoldNode bold:
                sb.Append("<b data-mw=\"bold\">");
                foreach (var child in bold.Children) sb.Append(SerializeNodeToHtml(child));
                sb.Append("</b>");
                break;
            case ItalicNode italic:
                sb.Append("<i data-mw=\"italic\">");
                foreach (var child in italic.Children) sb.Append(SerializeNodeToHtml(child));
                sb.Append("</i>");
                break;
            case HeadingNode heading:
                sb.Append($"<h{heading.Level} data-mw=\"heading\">");
                foreach (var child in heading.Children) sb.Append(SerializeNodeToHtml(child));
                sb.Append($"</h{heading.Level}>");
                break;
            case LinkNode link:
                string url;
                if (link.IsExternal)
                {
                    url = Uri.TryCreate(link.Target, UriKind.Absolute, out var uri) &&
                          (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
                        ? link.Target
                        : "#";
                }
                else
                {
                    url = $"/{link.Target}";
                }
                sb.Append($"<a href=\"{System.Net.WebUtility.HtmlEncode(url)}\" data-mw=\"link\">{System.Net.WebUtility.HtmlEncode(link.Display ?? "")}</a>");
                break;
            case CategoryNode:
                // Categories are metadata and not part of the visible HTML content
                break;
            case ListNode list:
                var tag = list.Type == ListType.Unordered ? "ul" : "ol";
                sb.Append($"<{tag}>");
                foreach (var child in list.Children) sb.Append(SerializeNodeToHtml(child));
                sb.Append($"</{tag}>");
                break;
            case ListItemNode listItem:
                sb.Append("<li>");
                foreach (var child in listItem.Children) sb.Append(SerializeNodeToHtml(child));
                sb.Append("</li>");
                break;
            case TableNode table:
                var userClass = ExtractClassAttribute(table.Attributes);
                var finalClass = string.IsNullOrEmpty(userClass)
                    ? "table table-bordered table-striped"
                    : $"table table-bordered table-striped {System.Net.WebUtility.HtmlEncode(userClass)}";
                sb.Append($"<table class=\"{finalClass}\">");
                foreach (var child in table.Children) sb.Append(SerializeNodeToHtml(child));
                sb.Append("</table>");
                break;
            case TableRowNode row:
                var rowSafeAttr = BuildSafeAttributes(row.Attributes);
                sb.Append(string.IsNullOrEmpty(rowSafeAttr) ? "<tr>" : $"<tr {rowSafeAttr}>");
                foreach (var child in row.Children) sb.Append(SerializeNodeToHtml(child));
                sb.Append("</tr>");
                break;
            case TableCellNode cell:
                var cellTag = cell.IsHeader ? "th" : "td";
                var cellSafeAttr = BuildSafeAttributes(cell.Attributes);
                sb.Append(string.IsNullOrEmpty(cellSafeAttr) ? $"<{cellTag}>" : $"<{cellTag} {cellSafeAttr}>");
                foreach (var child in cell.Children) sb.Append(SerializeNodeToHtml(child));
                sb.Append($"</{cellTag}>");
                break;
            case CodeNode code:
                sb.Append("<code>");
                foreach (var child in code.Children) sb.Append(SerializeNodeToHtml(child));
                sb.Append("</code>");
                break;
            case ParagraphNode paragraph:
                sb.Append("<p>");
                foreach (var child in paragraph.Children) sb.Append(SerializeNodeToHtml(child));
                sb.Append("</p>\n");
                break;
            case HtmlTagNode htmlTag:
                sb.Append(htmlTag.Tag);
                break;
            case TemplateNode template when template.TemplateName.Equals("Karte", StringComparison.OrdinalIgnoreCase):
                var lat = System.Net.WebUtility.HtmlEncode(template.Parameters.GetValueOrDefault("lat", "53.6761"));
                var lon = System.Net.WebUtility.HtmlEncode(template.Parameters.GetValueOrDefault("lon", "10.2736"));
                var zoom = System.Net.WebUtility.HtmlEncode(template.Parameters.GetValueOrDefault("zoom", "13"));
                var markerRaw = template.Parameters.GetValueOrDefault("marker", "");
                var markerAttr = !string.IsNullOrEmpty(markerRaw)
                    ? $" data-marker=\"{System.Net.WebUtility.HtmlEncode(markerRaw)}\""
                    : "";
                sb.Append($"<div data-lat=\"{lat}\" data-lon=\"{lon}\" data-zoom=\"{zoom}\"{markerAttr} style=\"height: 400px;\"></div>");
                break;
            case TemplateNode template:
                sb.Append($"<div class=\"mediawiki-template\" data-mw=\"template\" data-name=\"{System.Net.WebUtility.HtmlEncode(template.TemplateName)}\">");
                sb.Append(System.Net.WebUtility.HtmlEncode(string.Join(", ", template.Parameters.Select(p => $"{p.Key}={p.Value}"))));
                sb.Append("</div>");
                break;
            default:
                foreach (var child in node.Children) sb.Append(SerializeNodeToHtml(child));
                break;
        }
        return sb.ToString();
    }

    private static string ExtractClassAttribute(string attributes)
    {
        var m = System.Text.RegularExpressions.Regex.Match(attributes,
            @"class=""([^""]*)""|class='([^']*)'",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return m.Success ? (m.Groups[1].Value + m.Groups[2].Value).Trim() : "";
    }

    // Allows only class and style, strips event handlers and all other attributes.
    private static string BuildSafeAttributes(string rawAttributes)
    {
        var sb = new StringBuilder();
        var rx = System.Text.RegularExpressions.Regex.Matches(rawAttributes,
            @"(class|style)=""([^""]*)""|(?:class|style)='([^']*)'",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        foreach (System.Text.RegularExpressions.Match m in rx)
        {
            var name = m.Value.StartsWith("class", StringComparison.OrdinalIgnoreCase) ? "class" : "style";
            var val = m.Groups[2].Success ? m.Groups[2].Value : m.Groups[3].Value;
            sb.Append($"{name}=\"{System.Net.WebUtility.HtmlEncode(val)}\" ");
        }
        return sb.ToString().Trim();
    }

    public string ToWikiText(WikiNode node)
    {
        return SerializeNodeToWikiText(node);
    }

    private string SerializeNodeToWikiText(WikiNode node)
    {
        var sb = new StringBuilder();
        switch (node)
        {
            case TextNode text:
                sb.Append(text.Text);
                break;
            case BoldNode bold:
                sb.Append("'''");
                foreach (var child in bold.Children) sb.Append(SerializeNodeToWikiText(child));
                sb.Append("'''");
                break;
            case ItalicNode italic:
                sb.Append("''");
                foreach (var child in italic.Children) sb.Append(SerializeNodeToWikiText(child));
                sb.Append("''");
                break;
            case HeadingNode heading:
                var markers = new string('=', heading.Level);
                sb.Append(markers);
                foreach (var child in heading.Children) sb.Append(SerializeNodeToWikiText(child));
                sb.Append(markers);
                break;
            case LinkNode link:
                sb.Append("[[");
                sb.Append(link.Target);
                if (!string.IsNullOrEmpty(link.Display) && link.Display != link.Target)
                {
                    sb.Append("|");
                    sb.Append(link.Display);
                }
                sb.Append("]]");
                break;
            case CategoryNode category:
                sb.Append("[[Kategorie:");
                sb.Append(category.CategoryName);
                if (!string.IsNullOrEmpty(category.SortKey))
                {
                    sb.Append("|");
                    sb.Append(category.SortKey);
                }
                sb.Append("]]");
                break;
            case TemplateNode template:
                sb.Append("{{");
                sb.Append(template.TemplateName);
                foreach (var param in template.Parameters)
                {
                    sb.Append("|");
                    if (!string.IsNullOrEmpty(param.Key) && !int.TryParse(param.Key, out _))
                    {
                        sb.Append(param.Key);
                        sb.Append("=");
                    }
                    sb.Append(param.Value);
                }
                sb.Append("}}");
                break;
            case ListNode list:
                var prefix = list.Type == ListType.Unordered ? "*" : "#";
                foreach (var child in list.Children)
                {
                    if (child is ListItemNode listItem)
                    {
                        sb.Append(prefix);
                        sb.Append(" ");
                        foreach (var itemChild in listItem.Children) sb.Append(SerializeNodeToWikiText(itemChild));
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.Append(SerializeNodeToWikiText(child));
                    }
                }
                break;
            case ParagraphNode paragraph:
                foreach (var child in paragraph.Children) sb.Append(SerializeNodeToWikiText(child));
                sb.AppendLine();
                sb.AppendLine();
                break;
            case TableNode table:
                sb.AppendLine("{|");
                foreach (var child in table.Children) sb.Append(SerializeNodeToWikiText(child));
                sb.AppendLine("|}");
                break;
            case TableRowNode row:
                sb.AppendLine("|-");
                foreach (var child in row.Children) sb.Append(SerializeNodeToWikiText(child));
                break;
            case TableCellNode cell:
                sb.Append(cell.IsHeader ? "! " : "| ");
                foreach (var child in cell.Children) sb.Append(SerializeNodeToWikiText(child));
                sb.AppendLine();
                break;
            case CodeNode codeNode:
                sb.Append("<code>");
                foreach (var child in codeNode.Children) sb.Append(SerializeNodeToWikiText(child));
                sb.Append("</code>");
                break;
            case RootNode root:
                foreach (var child in root.Children) sb.Append(SerializeNodeToWikiText(child));
                break;
            default:
                foreach (var child in node.Children) sb.Append(SerializeNodeToWikiText(child));
                break;
        }
        return sb.ToString();
    }
}
