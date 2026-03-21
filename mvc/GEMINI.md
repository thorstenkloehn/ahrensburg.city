# MeinCMS - MVC Webanwendung

Dies ist die Haupt-Webanwendung für MeinCMS, entwickelt mit ASP.NET Core MVC 10.0 und PostgreSQL. Sie bietet die Benutzeroberfläche und die zentrale Wiki-Funktionalität.

## Projektübersicht

- **Framework**: .NET 10.0 (ASP.NET Core MVC)
- **Datenbank**: PostgreSQL (via Npgsql EF Core Provider)
- **ORM**: Entity Framework Core
- **Authentifizierung**: ASP.NET Core Identity
- **Frontend**: Razor Views, Bootstrap, jQuery, Markdig (Markdown), HtmlSanitizer

### Kernfunktionalitäten

- **Dynamisches Wiki-Routing**: Der `PageController` verarbeitet dynamische Wiki-Pfade mithilfe von Catch-all-Routenparametern (`{*slug}`).
- **Versionierung**: Wiki-Artikel werden versioniert. `WikiArtikel` speichert den Slug, und `WikiArtikelVersion` speichert den Inhalt (Markdown und HTML) sowie Metadaten wie Zeitstempel und Kategorien.
- **Sicherheit**: HtmlSanitizer wird verwendet, um generiertes HTML zu bereinigen und XSS-Angriffe zu verhindern.
- **Markdown-Unterstützung**: Markdig wird für die Umwandlung von Markdown in HTML verwendet.
- **Identity**: Standard ASP.NET Core Identity wird für die Benutzerverwaltung und Authentifizierung verwendet.

## Erstellen und Ausführen

### Voraussetzungen

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/)

### Einrichtung

1.  **Umgebung konfigurieren**:
    Kopieren Sie die Konfigurationsvorlagen an ihre tatsächlichen Speicherorte und aktualisieren Sie diese mit Ihren Datenbank-Zugangsdaten:
    ```bash
    cp _appsettings.json appsettings.json
    cp _appsettings.Development.json appsettings.Development.json
    ```

2.  **Migrationen anwenden**:
    Stellen Sie sicher, dass die Datenbank läuft, und führen Sie die EF Core Migrationen aus:
    ```bash
    dotnet ef database update
    ```

3.  **Anwendung starten**:
    ```bash
    dotnet run
    ```

## Entwicklungskonventionen

- **MVC-Muster**: Folgt strikt der Standardstruktur von ASP.NET Core MVC (`Controllers/`, `Models/`, `Views/`, `Data/`).
- **Slugs**: Der Zugriff auf Wiki-Seiten erfolgt über Slugs. Der `PageController` validiert Slugs mittels regulärer Ausdrücke (`^[a-zA-Z0-9/_-]+$`).
- **Gemeinsame Logik**: Wiederverwendbare Geschäftslogik sollte im `Services`-Projekt platziert werden (als Projektreferenz eingebunden).
- **Benennung**: 
    - **PascalCase** für Klassen, Methoden und öffentliche Eigenschaften.
    - **camelCase** für private Felder und lokale Variablen.
- **Styling**: Bevorzugen Sie Standard-CSS für benutzerdefinierte Stile, zu finden in `wwwroot/css/site.css`.

## Wichtige Dateien

- `Program.cs`: Der Einstiegspunkt der Anwendung und die Dienstekonfiguration.
- `mvc.csproj`: Projektdatei mit Abhängigkeiten und dem Zielframework.
- `Controllers/PageController.cs`: Haupt-Controller für das Rendern und Erstellen von Wiki-Seiten.
- `Models/WikiArtikel.cs`: Repräsentiert einen Eintrag einer Wiki-Seite.
- `Models/WikiArtikelVersion.cs`: Repräsentiert eine spezifische Inhaltsversion einer Wiki-Seite.
- `Data/ApplicationDbContext.cs`: EF Core Datenbankkontext.
