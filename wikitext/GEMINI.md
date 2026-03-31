# WikiText Parser (Wikitext) - Projektübersicht

Dieses Projekt ist eine eigenständige C#-Bibliothek zur Verarbeitung von MediaWiki-WikiText, entwickelt mit .NET 10.0. Es implementiert einen modernen, compiler-ähnlichen Ansatz (Tokenizer -> AST -> Serializer), um WikiText robust in HTML oder andere Formate zu transformieren.

## Architektur

Das System folgt einer klassischen Pipeline-Architektur:

1.  **Tokenizer (`MediaWikiTokenizer`)**: Zerlegt den WikiText-Input in eine Sequenz von atomaren `Token`-Objekten (z. B. `BoldStart`, `HeadingStart`, `Text`).
2.  **AST-Builder (`MediaWikiASTBuilder`)**: Verarbeitet die Token-Sequenz mithilfe eines Stack-basierten Algorithmus, um einen abstrakten Syntaxbaum (AST) aus `WikiNode`-Objekten zu erstellen.
3.  **Serializer (`MediaWikiASTSerializer`)**: Traversiert den AST, um das Zielformat zu generieren (z. B. HTML mit `ToHtml()` oder WikiText mit `ToWikiText()`).
4.  **Orchestrator (`MediaWikiParser`)**: Verbindet die Komponenten zu einem einfachen Interface.

## Hauptkomponenten

### Models (`Models/`)
-   **`Token.cs`**: Definiert `TokenType` und die `Token`-Klasse für die lexikalische Analyse.
-   **`WikiNode.cs`**: Enthält die Klassenhierarchie für den AST, darunter `BoldNode`, `HeadingNode`, `LinkNode`, `TableNode`, `ListNode` und `CategoryNode`.

### Parser (`Parser/`)
-   **`MediaWikiTokenizer.cs`**: Implementiert die Erkennung von MediaWiki-Syntaxelementen (einschließlich komplexer Heading-Logik und Tabellen-Shorthands).
-   **`MediaWikiASTBuilder.cs`**: Die Kernlogik zur Erstellung der Baumstruktur. Behandelt Verschachtelungen, automatische Listen-Erkennung und Tabellen-Strukturen.
-   **`MediaWikiASTSerializer.cs`**: Erzeugt sauberes HTML5 mit RDFa-ähnlichen Annotationen (`data-mw`) oder regeneriert WikiText für Round-tripping.
-   **`MediaWikiParser.cs`**: Bietet die primäre API für die Transformation.

### High-Level API
-   **`WikiTextGenerator.cs`**: Statische Utility-Klasse für schnellen Zugriff auf Serialisierungs-Funktionen direkt aus einem AST.

## Unterstützte Features

-   **Formatierung**: Fett (`'''`), Kursiv (`''`), Code (`<code>`).
-   **Struktur**: Überschriften (`=`, `==`, etc.), Listen (`*`, `#`), Tabellen (`{|`, `|-`, `|`, `!`, `|}`).
-   **Links**: Interne Links (`[[Ziel|Anzeige]]`), Externe Links (`[URL Beschreibung]`).
-   **Metadaten**: Kategorien (`[[Kategorie:Name]]`) mit Sortierschlüssel-Unterstützung.
-   **Templates**: Basis-Unterstützung für Vorlagen (`{{Name|Param1|Param2}}`).
-   **Sicherheit**: HTML-Encoding von Textinhalten während der Serialisierung.

## Erstellen und Ausführen

-   **Build**: `dotnet build`
-   **Tests**: (Falls vorhanden) `dotnet test`
-   **Abhängigkeiten**: Das Projekt ist minimal gehalten und nutzt nur Standard-.NET-Bibliotheken (System.Net.WebUtility für HTML-Encoding).

## Entwicklungskonventionen

-   **AST-First**: Jede Transformation sollte idealerweise über den AST laufen, um konsistente Ergebnisse zu gewährleisten.
-   **Surgical Updates**: Änderungen an der Grammatik erfordern meist Anpassungen sowohl im `Tokenizer` als auch im `ASTBuilder`.
-   **Daten-Attribute**: Das generierte HTML nutzt `data-mw`-Attribute, um semantische Informationen aus dem WikiText im DOM zu erhalten (wichtig für Editoren).
