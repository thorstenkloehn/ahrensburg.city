# MeinCMS - MVC Webanwendung

Dies ist die Haupt-Webanwendung für MeinCMS, entwickelt mit ASP.NET Core MVC 10.0 und PostgreSQL. Sie bietet die Benutzeroberfläche und die zentrale Wiki-Funktionalität.

## Projektübersicht

- **Framework**: .NET 10.0 (ASP.NET Core MVC)
- **Datenbank**: PostgreSQL (via Npgsql EF Core Provider)
- **ORM**: Entity Framework Core
- **Authentifizierung**: ASP.NET Core Identity (gehärtet)
- **Frontend**: Razor Views, Bootstrap 5, jQuery, Markdig (Markdown), HtmlSanitizer, DiffPlex.

### Kernfunktionalitäten

- **Wiki-only Routing**: Der `PageController` verarbeitet alle Pfade via Catch-all (`{*slug}`). Der `HomeController` wurde für eine geringere Angriffsfläche entfernt.
- **Versionierung & Diff**: Wiki-Artikel werden versioniert. Administratoren können Versionen vergleichen (Diff-Ansicht via DiffPlex) und wiederherstellen.
- **Sicherheit**: `HtmlSanitizer` bereinigt alle Markdown-generierten HTML-Inhalte. CSRF-Schutz und Security Header (HSTS/CSP) sind konfiguriert.
- **Metadaten**: Unterstützung für YAML Frontmatter zur Kategorisierung und für Schlagworte innerhalb der Markdown-Dateien.
- **Identity**: Registrierung ist deaktiviert. Zugriffsschutz auf Edit/History-Funktionen.

## Erstellen und Ausführen

### Voraussetzungen

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/)

### Einrichtung

1.  **Konfiguration**: `cp _appsettings.json appsettings.json` und PostgreSQL-String anpassen.
2.  **Migrationen**: `dotnet ef database update` ausführen.
3.  **Starten**: `dotnet run`

## Entwicklungskonventionen

- **Slugs**: Dynamische Pfade werden mittels Regex (`^[a-zA-Z0-9/_-]+$`) validiert.
- **Versionierung**: Jede Speicherung erzeugt eine neue `WikiArtikelVersion`.
- **Styling**: Standard-CSS in `wwwroot/css/site.css`.

## Wichtige Dateien

- `Program.cs`: Pipeline-Konfiguration und Diensteregistrierung.
- `Controllers/PageController.cs`: Zentraler Controller für das Wiki-Routing.
- `Models/WikiArtikel.cs`: Rumpfdaten eines Artikels (Slug).
- `Models/WikiArtikelVersion.cs`: Inhaltsdaten und Metadaten einer Version.
- `Data/ApplicationDbContext.cs`: Datenbankkontext.
