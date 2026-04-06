using System.Collections.Generic;

namespace Mardown.Models;

public abstract class MarkdownNode
{
    public List<MarkdownNode> Children { get; set; } = new();
}

public class RootNode : MarkdownNode { }

public class TextNode : MarkdownNode
{
    public string Text { get; set; } = string.Empty;
}

public class HeadingNode : MarkdownNode
{
    public int Level { get; set; }
}

public class BoldNode : MarkdownNode { }

public class ItalicNode : MarkdownNode { }

public class LinkNode : MarkdownNode
{
    public string Url { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class TemplateNode : MarkdownNode
{
    public string Name { get; set; } = string.Empty;
    public List<string> Parameters { get; set; } = new();
}

public class ListNode : MarkdownNode
{
    public bool IsOrdered { get; set; }
}

public class ListItemNode : MarkdownNode { }

public class ParagraphNode : MarkdownNode { }

public class CategoryNode : MarkdownNode
{
    public string Name { get; set; } = string.Empty;
}

public class TableNode : MarkdownNode { }

public class TableRowNode : MarkdownNode
{
    public bool IsHeader { get; set; }
}

public class TableCellNode : MarkdownNode { }

public class CodeInlineNode : MarkdownNode
{
    public string Code { get; set; } = string.Empty;
}

public class CodeBlockNode : MarkdownNode
{
    public string Code { get; set; } = string.Empty;
}
