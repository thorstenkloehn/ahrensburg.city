using System.Collections.Generic;

namespace mvc.Parser.Models;

public abstract class WikiNode
{
    public List<WikiNode> Children { get; set; } = [];
}

public class RootNode : WikiNode { }

public class TextNode : WikiNode
{
    public string Text { get; set; } = string.Empty;
}

public class BoldNode : WikiNode { }
public class ItalicNode : WikiNode { }

public class HeadingNode : WikiNode
{
    public int Level { get; set; }
}

public class LinkNode : WikiNode
{
    public string Target { get; set; } = string.Empty;
    public string? Display { get; set; }
    public bool IsExternal { get; set; }
}

public class CategoryNode : WikiNode
{
    public string CategoryName { get; set; } = string.Empty;
    public string? SortKey { get; set; }
}

public class TemplateNode : WikiNode
{
    public string TemplateName { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = [];
}

public class TableNode : WikiNode
{
    public string Attributes { get; set; } = string.Empty;
}

public class TableRowNode : WikiNode
{
    public string Attributes { get; set; } = string.Empty;
}

public class TableCellNode : WikiNode
{
    public bool IsHeader { get; set; }
    public string Attributes { get; set; } = string.Empty;
}

public enum ListType { Unordered, Ordered }
public class ListNode : WikiNode 
{
    public ListType Type { get; set; }
}
public class ListItemNode : WikiNode { }

public class ParagraphNode : WikiNode { }
