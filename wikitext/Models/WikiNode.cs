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

using System.Collections.Generic;

namespace Wikitext.Models;

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

public class CodeNode : WikiNode { }

public class HtmlTagNode : WikiNode
{
    public string Tag { get; set; } = string.Empty;
}
