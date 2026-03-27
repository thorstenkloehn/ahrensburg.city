using System.Text;
using mvc.Parser.Models;

namespace mvc.Parser;

public interface IMediaWikiASTSerializer
{
    string ToHtml(WikiNode node);
    string ToWikiText(WikiNode node);
}

public class MediaWikiASTSerializer : IMediaWikiASTSerializer
{
    public string ToHtml(WikiNode node)
    {
        var sb = new StringBuilder();
        foreach (var child in node.Children)
        {
            sb.Append(SerializeNodeToHtml(child));
        }
        return sb.ToString();
    }

    private string SerializeNodeToHtml(WikiNode node)
    {
        var sb = new StringBuilder();
        switch (node)
        {
            case TextNode text:
                sb.Append(System.Net.WebUtility.HtmlEncode(text.Text));
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
            default:
                foreach (var child in node.Children) sb.Append(SerializeNodeToHtml(child));
                break;
        }
        return sb.ToString();
    }

    public string ToWikiText(WikiNode node)
    {
        var sb = new StringBuilder();
        foreach (var child in node.Children)
        {
            sb.Append(SerializeNodeToWikiText(child));
        }
        return sb.ToString();
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
                if (link.Display != link.Target)
                {
                    sb.Append("|");
                    sb.Append(link.Display);
                }
                sb.Append("]]");
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
            default:
                foreach (var child in node.Children) sb.Append(SerializeNodeToWikiText(child));
                break;
        }
        return sb.ToString();
    }
}
