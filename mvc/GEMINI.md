# MeinCMS - MVC Webanwendung

Dies ist die Haupt-Webanwendung für MeinCMS, entwickelt mit ASP.NET Core MVC 10.0 und PostgreSQL. Sie bietet die Benutzeroberfläche und die zentrale Wiki-Funktionalität inklusive nativer Multi-Tenancy.

## Projektübersicht

- **Framework**: .NET 10.0 (ASP.NET Core MVC)
- **Multi-Tenancy**: Hostnamen-basierte Mandantentrennung (konfigurierbar in `appsettings.json`).
- **Datenbank**: PostgreSQL (via Npgsql EF Core Provider)
- **ORM**: Entity Framework Core mit Global Query Filters für die Mandantentrennung.
- **Authentifizierung**: ASP.NET Core Identity (gehärtet).

### Kernfunktionalitäten

- **Mandantentrennung**: Automatisches Filtern aller Inhalte basierend auf der Domain.
- **Wiki-only Routing**: Der `PageController` verarbeitet alle Pfade via Catch-all (`{*slug}`).
- **Versionierung & Diff**: Wiki-Artikel werden versioniert. Administratoren können Versionen vergleichen (Diff-Ansicht via DiffPlex) und wiederherstellen.
- **Suche**: Performante Volltextsuche über alle Wiki-Inhalte.
- **Sicherheit**: `HtmlSanitizer` bereigt alle generierten HTML-Inhalte. Strikte CSP ohne Inline-Skripte.
- **Metadaten**: Native Unterstützung für YAML Frontmatter.

## Erstellen und Ausführen

### Voraussetzungen

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/)

### Einrichtung

1.  **Konfiguration**: `cp _appsettings.json appsettings.json` und PostgreSQL-String sowie Mandanten-Mapping anpassen.
2.  **Migrationen**: `dotnet ef database update` ausführen.
3.  **Starten**: `dotnet run` (optional mit `--migrate` für automatische Migrationen).

## Entwicklungskonventionen

- **Multi-Tenancy**: Die `TenantId` wird automatisch in `ApplicationDbContext` gesetzt und gefiltert.
- **Slugs**: Unterstützung für Unicode, Leerzeichen und Sonderzeichen (`&`, `:`, `(`, `)`, `!`, `"`, `,`, `` ` ``, `–`). Eindeutigkeit ist pro Mandant gegeben.
- **Versionierung**: Jede Speicherung erzeugt eine neue `WikiArtikelVersion`.

## Wichtige Dateien

- `Program.cs`: Pipeline-Konfiguration, DI für `ITenantService`.
- `Services/TenantService.cs`: Logik zur Mandantenerkennung aus dem Hostnamen.
- `Controllers/PageController.cs`: Zentraler Controller für das Wiki-Routing.
- `Models/WikiArtikel.cs`: Rumpfdaten eines Artikels (TenantId, Slug).
- `Data/ApplicationDbContext.cs`: Datenbankkontext mit Global Query Filters.
