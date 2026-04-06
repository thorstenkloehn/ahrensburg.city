namespace Mardown.Models;

public enum MarkdownTokenType
{
    Text,
    Heading,
    Bold,
    Italic,
    Link,
    Template,
    List,
    Newline,
    Category,
    TableRow,
    TableDivider,
    CodeInline,
    CodeBlock
}

public class MarkdownToken
{
    public MarkdownTokenType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public int Level { get; set; } // For headings or nested lists
    public List<string> Parameters { get; set; } = new(); // For templates
}
