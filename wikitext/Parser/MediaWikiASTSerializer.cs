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
                var url = link.IsExternal ? link.Target : $"/{link.Target}";
                sb.Append($"<a href=\"{url}\" data-mw=\"link\">{link.Display}</a>");
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
                var attributes = table.Attributes;
                var finalClass = "table table-bordered table-striped";
                
                if (attributes.Contains("class=\""))
                {
                    // Merge class attribute
                    attributes = attributes.Replace("class=\"", $"class=\"{finalClass} ");
                }
                else if (attributes.Contains("class='"))
                {
                    attributes = attributes.Replace("class='", $"class='{finalClass} ");
                }
                else
                {
                    attributes = $"class=\"{finalClass}\" " + attributes;
                }

                attributes = attributes.Trim();
                sb.Append($"<table {attributes}>");
                foreach (var child in table.Children) sb.Append(SerializeNodeToHtml(child));
                sb.Append("</table>");
                break;
            case TableRowNode row:
                var rowAttr = row.Attributes.Trim();
                sb.Append($"<tr{(string.IsNullOrEmpty(rowAttr) ? "" : " " + rowAttr)}>");
                foreach (var child in row.Children) sb.Append(SerializeNodeToHtml(child));
                sb.Append("</tr>");
                break;
            case TableCellNode cell:
                var cellTag = cell.IsHeader ? "th" : "td";
                var cellAttr = cell.Attributes.Trim();
                sb.Append($"<{cellTag}{(string.IsNullOrEmpty(cellAttr) ? "" : " " + cellAttr)}>");
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
