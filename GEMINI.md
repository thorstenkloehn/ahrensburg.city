# ahrensburg-city (MeinCMS) - Projektübersicht

Dieses Projekt (ehemals MeinCMS) ist ein leichtgewichtiges Content-Management-System (CMS) mit Wiki-ähnlicher Funktionalität und nativer **Multi-Tenancy** (Mandantenfähigkeit), entwickelt mit ASP.NET Core MVC 10.0 und PostgreSQL.

## Status (Stand 25.03.2026)

- **Go-Live bereit:** Das System hat das abschließende Sicherheits-Audit erfolgreich bestanden.
- **Multi-Tenancy:** Optimierte Unterstützung für mehrere Mandanten (z. B. `localhost` und `doc.localhost`) mit dynamischen Datenbank-Filtern.
- **Benutzerverwaltung:** Verwaltung erfolgt ausschließlich über das `UserAdmin`-Tool.
- **Rechtssicherheit:** Integrierter Cookie-Banner zur Information der Nutzer (DSGVO/TDDDG-konform).
- **Datenbank:** Aktueller Schema-Stand inkl. Migration `MultiTenancy` (März 2026). Eindeutiger Index für `(TenantId, Slug)` ist aktiv.
- **UI & UX:** Modernisierte 404-Fehlerseite ("Kein Artikel vorhanden") und erweiterter Footer (Impressum, Datenschutz).

## Architektur

- **`mvc/`**: Die Haupt-Webanwendung (Wiki-only).
  - **Multi-Tenancy**: Dynamische Mandantenerkennung via Hostname und **Request-spezifische Filterung** im `ApplicationDbContext`.
  - **Controller**: `PageController.cs` verwaltet das gesamte Routing.
  - **Models**: `WikiArtikel` und `WikiArtikelVersion` (mit `[XmlIgnore]` zur Serialisierung).
- **`Services/`**: Geschäftslogik, `PageService`, `TenantService` und `Blogs`.
- **`UserAdmin/`**: Konsolenanwendung für die Verwaltung der Administratoren.
- **`backup/`**: Spezialisiertes Kommandozeilen-Tool für XML-Backup/Export und Import (Upsert-Logik).

## Technologien

- **Framework**: .NET 10.0 (ASP.NET Core)
- **Datenbank**: PostgreSQL (via Npgsql)
- **Bibliotheken**: Markdig, HtmlSanitizer, DiffPlex, YamlDotNet.
- **Dokumentation**: DocFX (statischer Export nach `docs/`).

## Erstellen und Ausführen

### Ausführen

- **Web**: `dotnet run --project mvc`
- **Admin**: `dotnet run --project UserAdmin`
- **Backup Export**: `dotnet run --project backup -- export --full`
- **Backup Import**: `dotnet run --project backup -- import backup.xml`

## Entwicklungskonventionen

- **Multi-Tenancy**: Jede Abfrage wird automatisch nach dem aktuellen Mandanten gefiltert. Die `TenantId` wird dynamisch pro Request ermittelt.
- **Wiki-Routing**: Catch-all-Route `{*slug}` im `PageController`.
- **404 Handling**: Fehler werden über die `NotFound404`-View mit "Kein Artikel vorhanden"-Meldung abgefangen.
- **Cookie-Banner**: Informiert Nutzer über Cookie-Nutzung und verlinkt auf Datenschutz/Impressum.

## TODO / Roadmap

- [x] Implementierung einer robusten Bearbeitung und Versionierung (inkl. Diff-Ansicht).
- [x] **Multi-Tenancy:** Native Mandantenfähigkeit via Hostname-Erkennung.
- [x] **Sicherheit:** Security Header (CSP, HSTS Tuning) und gehärtete Password Policy.
- [x] **Infrastruktur:** Solution umbenannt in `ahrensburg-city.slnx`.
- [x] **Infrastruktur:** Automatisierter XML-Export/Import (Upsert-Logik) im `backup`-Projekt.
- [x] **Dokumentation:** Vollständige DocFX-Einrichtung mit statischem Export.
- [x] **Rechtssicherheit:** Cookie-Banner Implementierung.
- [ ] Implementierung von Unit- und Integrationstests (xUnit).
- [ ] Performance-Monitoring der Diff-Funktion bei großen Artikeln.
- [ ] Unterstützung für Themes oder CSS-Anpassungen pro Mandant.
