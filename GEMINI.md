# MeinCMS - Projektübersicht

MeinCMS ist ein leichtgewichtiges Content-Management-System (CMS) mit Wiki-ähnlicher Funktionalität und nativer **Multi-Tenancy** (Mandantenfähigkeit), entwickelt mit ASP.NET Core MVC 10.0 und PostgreSQL.

## Status (Stand 24.03.2026)

- **Go-Live bereit:** Das System hat das abschließende Sicherheits-Audit am 24. März 2026 erfolgreich bestanden.
- **Multi-Tenancy:** Unterstützung für mehrere Mandanten (z. B. `ahrensburg.city` und `doc.ahrensburg.city`) auf einer Instanz.
- **Sicherheits-Audits:** Mehrere Audits im März 2026 (siehe `bericht/`). Fokus auf XSS-Prävention, Authentifizierung und Mandantentrennung.
- **Benutzerverwaltung:** Öffentliche Registrierung deaktiviert. Verwaltung erfolgt ausschließlich über das `UserAdmin`-Tool.
- **Datenbank:** Aktueller Schema-Stand inkl. Migration `MultiTenancy` (März 2026). Eindeutiger Index für `(TenantId, Slug)` ist aktiv.

## Architektur

- **`mvc/`**: Die Haupt-Webanwendung (Wiki-only).
  - **Multi-Tenancy**: Dynamische Mandantenerkennung via Hostname (konfigurierbar in `appsettings.json`).
  - **Controller**: `PageController.cs` verwaltet das gesamte Routing (Catch-all). `HomeController` wurde entfernt.
  - **Models**: `WikiArtikel` (TenantId, Slug) und `WikiArtikelVersion` (Inhalt, Metadaten).
  - **Data**: Entity Framework Core `ApplicationDbContext` mit **Global Query Filters** zur strikten Mandantentrennung.
  - **Identity**: ASP.NET Core Identity mit gehärteter Password-Policy (12+ Zeichen, Lockout). Globaler Admin-Pool für alle Mandanten.
- **`Services/`**: Geschäftslogik, `PageService` (Wiki-Logik), `TenantService` (Mandanten-Logik) und `Blogs`.
- **`UserAdmin/`**: Konsolenanwendung für administrative Aufgaben (Passwort-maskierte Eingabe).

## Technologien

- **Framework**: .NET 10.0 (ASP.NET Core)
- **Datenbank**: PostgreSQL (via Npgsql)
- **Bibliotheken**: Markdig (Markdown), HtmlSanitizer (XSS-Schutz), DiffPlex (Versions-Vergleich), YamlDotNet (Metadaten).
- **UI**: Razor Views, Bootstrap 5, jQuery.

## Dokumentation

- **DocFX**: API-Dokumentation (konfiguriert in `docfx.json`).
- **Sicherheitsberichte**: In `/bericht` (chronologisch sortiert).

## Erstellen und Ausführen

### Voraussetzungen

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/)

### Einrichtung

1.  **Umgebung**: `cp mvc/_appsettings.json mvc/appsettings.json` und Verbindungsdaten sowie Mandanten-Mapping anpassen.
2.  **Datenbank**: Migrationen anwenden mit `dotnet ef database update --project mvc`.

### Ausführen

- **Web**: `dotnet run --project mvc`
- **Admin**: `dotnet run --project UserAdmin`

## Entwicklungskonventionen

- **Multi-Tenancy**: Jede Abfrage wird automatisch nach dem aktuellen Mandanten gefiltert.
- **Wiki-Routing**: Catch-all-Route `{*slug}` im `PageController`.
- **Versionierung**: Jede Änderung erzeugt eine neue `WikiArtikelVersion`. Historie/Diff ist für Admins verfügbar.
- **Metadaten**: Werden via YAML Frontmatter direkt im Markdown-Inhalt gespeichert und extrahiert.

## TODO / Roadmap

- [x] Implementierung einer robusten Bearbeitung und Versionierung (inkl. Diff-Ansicht).
- [x] **Multi-Tenancy:** Native Mandantenfähigkeit via Hostname-Erkennung.
- [x] Sicherheits-Audits und Go-Live Freigabe.
- [x] **Sicherheit:** Password Policy (min. 12 Zeichen) und Account Lockout.
- [x] **Sicherheit:** Security Header (CSP, HSTS Tuning).
- [x] **Datenbank:** Eindeutiger Index für `(TenantId, Slug)`.
- [x] **Architektur:** Entfernung unnötiger Controller (`HomeController`).
- [x] **Sicherheit:** Deaktivierung der öffentlichen Registrierung.
- [ ] Implementierung von Unit- und Integrationstests (xUnit).
- [ ] Performance-Monitoring der Diff-Funktion bei großen Artikeln.
- [ ] Unterstützung für Themes oder CSS-Anpassungen pro Mandant.
