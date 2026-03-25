# ahrensburg.city (MeinCMS) - Projektübersicht

Dieses Projekt ist ein leichtgewichtiges Content-Management-System (CMS) mit Wiki-ähnlicher Funktionalität und nativer **Multi-Tenancy** (Mandantenfähigkeit), entwickelt mit ASP.NET Core MVC 10.0 und PostgreSQL.

## Status (Stand 26.03.2026)

- **Go-Live bereit:** Das System hat das abschließende Sicherheits-Audit erfolgreich bestanden.
- **Branding:** Hauptmandant wurde auf **ahrensburg.city** umgestellt.
- **Multi-Tenancy:** Vollständig normalisierte Datenbank. Automatische Korrektur von leeren `TenantId`-Feldern zu `"main"` via Backup-Tool implementiert.
- **Backup 2.0:** 
  - Unterstützung für **YAML** und **XML**.
  - **Speichereffizient**: HTML-Inhalt wird beim Export weggelassen und beim Import regeneriert (ca. 70% Ersparnis).
  - Flexible Dateinamen-Wahl via CLI.
- **Deployment:** Unterstützung für **Unix Domain Sockets** (`/run/meincms.sock`) für hochperformanten Betrieb hinter Nginx.
- **Sicherheit:** Integration von `HtmlSanitizer`, gehärtete Identity-Policies und mandantenspezifische Datenisolation.

## Architektur

- **`mvc/`**: Die Haupt-Webanwendung (Wiki-only).
  - **Multi-Tenancy**: Dynamische Mandantenerkennung via Hostname und **Request-spezifische Filterung** im `ApplicationDbContext`.
  - **Models**: `WikiArtikel` und `WikiArtikelVersion` (mit `[XmlIgnore]` und `[YamlIgnore]` für redundante Felder).
- **`Services/`**: Geschäftslogik, `PageService`, `TenantService` und `Blogs`.
- **`UserAdmin/`**: Konsolenanwendung für die Verwaltung der Administratoren.
- **`backup/`**: Spezialisiertes Tool für YAML/XML-Export und Import mit Normalisierungs-Logik.

## Technologien

- **Framework**: .NET 10.0 (ASP.NET Core)
- **Datenbank**: PostgreSQL (via Npgsql)
- **Bibliotheken**: Markdig, HtmlSanitizer, DiffPlex, YamlDotNet.

## Erstellen und Ausführen

- **Web**: `dotnet run --project mvc`
- **Admin**: `dotnet run --project UserAdmin`
- **Backup Export**: `dotnet run --project backup -- export meine_daten.yaml --full`
- **Backup Import**: `dotnet run --project backup -- import meine_daten.yaml`

## Entwicklungskonventionen

- **Multi-Tenancy**: Jede Abfrage wird automatisch nach dem aktuellen Mandanten gefiltert. Die `TenantId` wird dynamisch pro Request ermittelt.
- **Normalisierung**: Leere `TenantId` in der Datenbank gelten als `"main"`.
- **Sicherheit**: Alle Markdown-Inhalte müssen vor der Anzeige via `HtmlSanitizer` bereinigt werden.

## TODO / Roadmap

- [x] **Multi-Tenancy:** Native Mandantenfähigkeit via Hostname-Erkennung.
- [x] **Backup:** XML/YAML-Export mit HTML-Regenerierung und Tenant-Normalisierung.
- [x] **Sicherheit:** Security Header (CSP, HSTS Tuning) und gehärtete Password Policy.
- [x] **Rechtssicherheit:** Cookie-Banner Implementierung.
- [x] **Themes:** Unterstützung für Themes oder CSS-Anpassungen pro Mandant.
- [x] **Snyk Integration:** Vorbereitungen für automatisiertes Security-Scanning getroffen.
- [ ] **Redirects:** Implementierung eines Alias/Redirect-Systems für umbenannte Slugs.
- [ ] **Admin UI:** Integration einer webbasierten Benutzerverwaltung.
