# Mardown Parser - Projektübersicht

Eine leichtgewichtige, modular aufgebaute Markdown-Parser-Bibliothek für .NET 10.0. Dieser Parser nutzt einen compiler-ähnlichen Ansatz (Tokenizer -> AST -> Serializer) und verzichtet vollständig auf externe Abhängigkeiten wie Markdig.

## Architektur

Das System ist in drei Hauptphasen unterteilt:

1.  **Tokenizer (`MarkdownTokenizer`)**:
    - Zerlegt den Markdown-Quelltext mittels regulärer Ausdrücke in Tokens.
    - Unterstützt Zeilen-basierte Erkennung (Headings, Listen) und Inline-Erkennung (Fett, Kursiv, Links).
2.  **AST-Builder (`MarkdownASTBuilder`)**:
    - Erzeugt aus der Token-Sequenz einen abstrakten Syntaxbaum (`MarkdownNode`).
    - Ermöglicht eine logische Abbildung der Dokumentstruktur.
3.  **Serializer (`MarkdownASTSerializer`)**:
    - Traversiert den AST und generiert das Zielformat (aktuell HTML).
    - Nutzt `HttpUtility` für sicheres Encoding.

## Unterstützte Features

- **Überschriften**: `# H1` bis `###### H6`.
- **Formatierung**: `**Fett**` oder `__Fett__`, `*Kursiv*` oder `_Kursiv_`.
- **Links**: `[Label](URL)`.
- **Listen**: Einfache ungeordnete Listen mit `*`, `+` oder `-`.
- **Templates**: Unterstützung für MediaWiki-ähnliche Templates `{{Name|Parameter}}`.
- **Zeilenumbrüche**: Automatische Behandlung von Newlines.

## Verwendung

```csharp
var parser = new Mardown.Parser.MarkdownParser();
string html = parser.ToHtml("# Mein Titel\n**Fetter Text**");
```

## Modularität und Erweiterbarkeit

- **Neue Elemente**: Einfach einen neuen `MarkdownTokenType` hinzufügen, im `Tokenizer` via Regex erkennen und im `ASTBuilder` / `Serializer` verarbeiten.
- **Templates**: Der `TemplateNode` ist so konzipiert, dass er rekursiv erweitert werden kann, um komplexe Logik innerhalb von Vorlagen zu unterstützen.
- **Performance**: Durch die Verwendung von `StringBuilder` und optimierten Regex-Suchen ist der Parser auch für größere Dokumente geeignet.

## Tests

Das Projekt wird durch Unit-Tests in `Mardown.Tests` abgesichert.
Führen Sie die Tests mit `dotnet test Mardown.Tests` aus.
