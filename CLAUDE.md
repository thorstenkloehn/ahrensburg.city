# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Projektübersicht

**MeinCMS** — ein mandantenfähiges Wiki/CMS auf Basis von ASP.NET Core 10 und PostgreSQL. Zwei Mandanten laufen auf derselben Instanz: `wissen-ahrensburg.de` (Stadtinhalte, `"main"`) und `doc.wissen-ahrensburg.de` (technische Dokumentation, `"doc"`).

## Befehle

**Ersteinrichtung:**
```bash
cp mvc/_appsettings.json mvc/appsettings.json   # dann PostgreSQL-String und Mandanten-Mapping anpassen
dotnet ef database update --project mvc
```

**Build & Starten:**
```bash
dotnet build
dotnet run --project mvc
dotnet run --project mvc -- --migrate   # starten mit automatischer DB-Migration
```

**Tests:**
```bash
dotnet test mvc.Tests
dotnet test Mardown.Tests
dotnet test mvc.Tests --filter "FullyQualifiedName~PageServiceTests"   # einzelnen Test/Klasse ausführen
```

**Datenbankmigrationen:**
```bash
dotnet ef database update --project mvc
```

**CLI-Tools:**
```bash
dotnet run --project UserAdmin                            # Benutzer-/Rollenverwaltung
dotnet run --project backup -- export <file.yaml> --full  # Backup-Export
dotnet run --project backup -- import <file.yaml>          # Backup-Import
dotnet run --project backup -- repair                      # HTML nach Parser-Änderungen neu generieren
```

**Dokumentation:**
```bash
docfx   # erfordert dotnet-tools.json (v2.78.5)
```

## Architektur

### Multi-Tenancy

Der Mandant wird pro Request aus dem HTTP-Hostnamen durch den `TenantService` ermittelt. `ApplicationDbContext` wendet EF Core Global Query Filter auf `WikiArtikel`, `WikiArtikelVersion` und `WikiCategory` an — alle werden automatisch nach `TenantId` gefiltert. Es gibt einen zusammengesetzten Unique-Index auf `(TenantId, Slug)`.

### Parser-Pipeline

Zwei unabhängige Content-Parser teilen dieselbe Compiler-Architektur:

- **MediaWiki-Parser** (`wikitext/`): verarbeitet WikiText-Syntax (Templates, Kategorien, Links, Inline-HTML). Pipeline: `MediaWikiTokenizer` → `MediaWikiASTBuilder` → `MediaWikiASTSerializer`
- **Markdown-Parser** (`Mardown/`): unterstützt YAML-Frontmatter, Tabellen, Code-Blöcke. Pipeline: `MarkdownTokenizer` → `MarkdownASTBuilder` → `MarkdownASTSerializer`

`PageService` erkennt das Format automatisch und rendert es vor Speicherung und Anzeige zu bereinigtem HTML via `HtmlSanitizer`. Gerenderte Artikel werden in `IMemoryCache` unter dem Schlüssel `wiki_{tenantId}_{slug}` gecacht; der Cache-Eintrag wird bei Schreib- und Wiederherstellungsoperationen invalidiert.

### Routing

- `PageController` verarbeitet alle Wiki-Pfade via Catch-all (`{*slug}`).
- `BlogController` und `NewsController` haben dedizierte Routen; Inhalte werden über `IPageService.GetBlogArticlesAsync` / `GetNewsArticlesAsync` geladen.
- Versionsvergleich (Diff) nutzt DiffPlex (`GenerateDiff` in `IPageService`).

### Projekte im Überblick

| Projekt | Zweck |
|---|---|
| `mvc/` | Haupt-Webanwendung (ASP.NET Core MVC) |
| `mvc.Tests/` | xUnit-Tests (inkl. `WebApplicationFactory`-Integrationstests) |
| `wikitext/` | MediaWiki-WikiText-Parser-Bibliothek |
| `Mardown/` | Markdown-Parser-Bibliothek |
| `Mardown.Tests/` | xUnit-Tests für den Markdown-Parser |
| `Services/` | Gemeinsame Service-Bibliothek (aktuell: `Blogs`-Stub) |
| `backup/` | CLI: YAML/XML-Export, -Import und HTML-Reparatur |
| `UserAdmin/` | CLI: Benutzer- und Rollenverwaltung |
| `TempMigrate/` | Einmalige Mandanten-Migrationshilfe (nicht für neue Aufgaben verwenden) |

### Konfigurationsreihenfolge

`Program.cs` lädt die Konfiguration in dieser Prioritätsreihenfolge:
1. `appsettings.json` (Wurzelverzeichnis)
2. `config/appsettings.json`
3. `appsettings.{Environment}.json`
4. `config/appsettings.{Environment}.json`
5. Umgebungsvariablen

Der Unix Domain Socket für die Nginx-Integration wird über `Kestrel:UnixSocket` in den appsettings konfiguriert (Pfad: `/run/meincms.sock`).

### Sicherheits-Hardening (nicht abschwächen)

- **CSRF**: `AutoValidateAntiforgeryTokenAttribute` global aktiviert
- **HTML-Ausgabe**: wird immer über `HtmlSanitizer` bereigt
- **CSP**: Strikte Content-Security-Policy; **keine Inline-Skripte** erlaubt (Logik in `site.js` / `editor.js`)
- **HSTS / X-Frame-Options / Referrer-Policy**: gesetzt in der Middleware in `Program.cs`
- **Passwort-Richtlinie**: min. 12 Zeichen, Groß-/Kleinschreibung, Ziffer, Sonderzeichen
- **Kontosperrung**: 5 Fehlversuche → 15 Minuten Sperre
- **Caching**: Globale No-Cache-Middleware setzt `Cache-Control: no-cache, no-store` für alle Responses.

### Dokumentation (`Anleitung/`)

- `ErsteProduktionServer.md`: Schritt-für-Schritt Anleitung für das erste Deployment.
- `Aktuallisieren.md`: Prozedur für Updates auf dem Server.
- `ssh.md`: Befehle für SSH/SCP-basierte Backups.

### Datenmodell

- `WikiArtikel` — Artikel mit `Slug`, `TenantId`, `NamespaceId` und Navigation zu Versionen
- `WikiArtikelVersion` — unveränderliche Versions-Snapshots (Inhalt + Zeitstempel)
- `WikiNamespace` — Namespace-Entität in der DB (Main, Category, Template, …); `NamespaceId` ist ein FK auf diese Tabelle
- `WikiCategory` — hierarchische Kategorien, gefiltert nach `TenantId`

### Hinweise zum Backup-Tool

Das Backup-Format schließt gerendertes HTML aus (ca. 70% Größenersparnis). Beim Import wird das HTML neu generiert. Nach jeder Parser-Änderung muss `dotnet run --project backup -- repair` ausgeführt werden, um das gespeicherte HTML aller Artikel neu zu erzeugen.
