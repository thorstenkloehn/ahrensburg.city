# MeinCMS - Projektübersicht

MeinCMS ist ein leichtgewichtiges Content-Management-System (CMS) mit Wiki-ähnlicher Funktionalität, entwickelt mit ASP.NET Core MVC 10.0 und PostgreSQL.

## Status (Stand 22.03.2026)

- **Sicherheits-Audits:** Zwei umfassende Audits wurden am 20. und 21. März 2026 durchgeführt (siehe `bericht/`).
- **Benutzerverwaltung:** Die öffentliche Registrierung ist deaktiviert. Benutzer werden ausschließlich über das `UserAdmin`-Tool verwaltet.
- **Datenbank:** Aktueller Schema-Stand inkl. Migration `Hallo` (pflichtmäßige WikiArtikel-Zuweisung für Versionen).

## Architektur

- **`mvc/`**: Die Haupt-Webanwendung, die das MVC-Muster implementiert.
  - **Controller**: Verwalten das Routing und die Benutzerinteraktion (z. B. `PageController.cs`).
  - **Models**: Definieren die Datenstrukturen (`WikiArtikel`, `WikiArtikelVersion`).
  - **Data**: Entity Framework Core `ApplicationDbContext` und Migrationen.
  - **Identity**: Integrierte ASP.NET Core Identity für die Authentifizierung.
- **`Services/`**: Eine Klassenbibliothek für gemeinsame Geschäftslogik und Dienste (z. B. `PageService`, `Blogs`).
- **`UserAdmin/`**: Eine Konsolenanwendung für administrative Aufgaben, wie das Erstellen und Auflisten von Benutzern.

## Technologien

- **Framework**: .NET 10.0 (ASP.NET Core)
- **Datenbank**: PostgreSQL (via Npgsql EF Core Provider)
- **ORM**: Entity Framework Core
- **Authentifizierung**: ASP.NET Core Identity
- **UI**: Razor Views, Bootstrap, jQuery, Markdig (Markdown), HtmlSanitizer

## Dokumentation

- **DocFX**: Wird verwendet, um die API-Dokumentation aus dem Quellcode zu generieren. Die Konfiguration befindet sich in `docfx.json`.
- **Markdown**: Projektdokumentation in `/doc` und `/docs`. Sicherheitsberichte in `/bericht`.

## Erstellen und Ausführen

### Voraussetzungen

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/)

### Einrichtung

1.  **Umgebung konfigurieren**:
    Kopieren Sie die Vorlagen der Konfigurationsdateien:
    ```bash
    cp mvc/_appsettings.Development.json mvc/appsettings.Development.json
    cp mvc/_appsettings.json mvc/appsettings.json
    ```
    Aktualisieren Sie `mvc/appsettings.json` mit Ihrem PostgreSQL-Verbindungsstring.

2.  **Datenbank-Setup**:
    Beispiel für Linux:
    ```bash
    sudo -u postgres -i
    createdb -E UTF8 -O ihr_benutzer mvc
    psql -d mvc -c "GRANT ALL PRIVILEGES ON DATABASE mvc TO ihr_benutzer"
    exit
    ```

3.  **Migrationen anwenden**:
    ```bash
    dotnet ef database update --project mvc
    ```

### Ausführen der Anwendungen

- **Webanwendung**: `dotnet run --project mvc`
- **Benutzeradministrations-Tool**: `dotnet run --project UserAdmin`

## Entwicklungskonventionen

- **Wiki-Routing**: `PageController` verarbeitet dynamische Wiki-Pfade via Catch-all-Routenparameter (`{*slug}`). Slugs sind Regex-validiert.
- **Versionierung**: Wiki-Inhalte werden versioniert gespeichert. Eine Version ist zwingend einem Artikel zugeordnet (Migration `Hallo`).
- **Kategorien**: `WikiArtikelVersion` unterstützt eine Liste von Kategorien (`Kategorie`-Property).
- **Gemeinsame Logik**: Geschäftslogik gehört in das Projekt `Services`.

## TODO / Roadmap

- [x] Implementierung einer robusten Bearbeitung und Versionierung von Wiki-Seiten.
- [x] Durchführung von Sicherheits-Audits (März 2026).
- [x] **Sicherheit:** Implementierung einer Password Policy (min. 12 Zeichen) und Account Lockout.
- [x] **Sicherheit:** Implementierung von Security Headern (CSP, HSTS Tuning).
- [x] **Datenbank:** Eindeutigen Index für `Slug` in `WikiArtikel` hinzufügen.
- [x] **Features:** Korrektur und Vorbereitung der Blog-Funktion (ehemals `Bloogs`).
- [x] **Sicherheit:** Maskierung der Passworteingabe im UserAdmin-Tool.
- [ ] **Features:** Implementierung eines Datei-/Bildupload-Systems.
- [ ] Hinzufügen von Unit- und Integrationstests.
- [ ] Unterstützung für Themes oder CSS-Anpassungen.
