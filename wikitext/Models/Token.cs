namespace Wikitext.Models;

public enum TokenType
{
    Text,
    BoldStart, // '''
    BoldEnd,
    ItalicStart, // ''
    ItalicEnd,
    HeadingStart, // ==, ===, etc.
    HeadingEnd,
    LinkStart, // [[
    LinkEnd, // ]]
    ExternalLinkStart, // [
    ExternalLinkEnd, // ]
    TemplateStart, // {{
    TemplateEnd, // }}
    TableStart, // {|
    TableEnd, // |}
    TableRow, // |-
    TableCell, // |
    TableHeader, // !
    CategoryStart, // [[Kategorie: or [[Category:
    BulletList, // *
    NumberedList, // #
    NewLine,
    CodeStart, // <code>
    CodeEnd, // </code>
    TagStart, // <
    TagEnd // >
}

public class Token
{
    public TokenType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public int Position { get; set; }
    public int Length { get; set; }

    public Token(TokenType type, string value, int position, int length)
    {
        Type = type;
        Value = value;
        Position = position;
        Length = length;
    }

    public override string ToString() => $"[{Type}] '{Value}' at {Position}";
}
