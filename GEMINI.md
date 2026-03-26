# ahrensburg.city (MeinCMS) - Projektübersicht

Dieses Projekt ist ein leichtgewichtiges Content-Management-System (CMS) mit Wiki-ähnlicher Funktionalität und nativer **Multi-Tenancy** (Mandantenfähigkeit), entwickelt mit ASP.NET Core MVC 10.0 und PostgreSQL.

## Status (Stand 26.03.2026)

- **Go-Live bereit:** Das System hat das abschließende Sicherheits-Audit erfolgreich bestanden.
- **Branding:** Hauptmandant wurde auf **ahrensburg.city** umgestellt.
- **Multi-Tenancy:** Vollständig normalisierte Datenbank. Automatische Trennung von technischem Dokumentationsinhalt (`doc`) und Ahrensburg-spezifischen Inhalten (`main`) via Tenant-Migration-Tool durchgeführt.
- **Slug-Validierung:** Unterstützung für Unicode, Leerzeichen und Sonderzeichen wie `&`, `:`, `(`, `)`, `!`, `"`, `,`, `` ` ``, und `–`. Dies ermöglicht natürliche Titel wie "Geschichte & Allgemeines".
- **Backup 2.0:** 
  - Unterstützung für **YAML** und **XML**.
  - **Speichereffizient**: HTML-Inhalt wird beim Export weggelassen und beim Import regeneriert (ca. 70% Ersparnis).
- **Deployment:** Unterstützung für **Unix Domain Sockets** (`/run/meincms.sock`) für hochperformanten Betrieb hinter Nginx.
- **Sicherheit:** Integration von `HtmlSanitizer`, gehärtete Identity-Policies, CSRF-Schutz und strikte Content-Security-Policy (CSP).

## Architektur

- **`mvc/`**: Die Haupt-Webanwendung (Wiki-only).
  - **Multi-Tenancy**: Dynamische Mandantenerkennung via Hostname und **Request-spezifische Filterung** im `ApplicationDbContext`.
  - **Models**: `WikiArtikel` und `WikiArtikelVersion`.
- **`Services/`**: Geschäftslogik, `PageService`, `TenantService`.
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
- **Normalisierung**: Standard-Mandant ist `"main"`. Technischer Content wird dem Mandanten `"doc"` zugeordnet.
- **Sicherheit**: Alle Markdown-Inhalte werden vor der Anzeige via `HtmlSanitizer` bereinigt. CSRF-Schutz ist global aktiviert.

## TODO / Roadmap

- [x] **Multi-Tenancy:** Native Mandantenfähigkeit via Hostname-Erkennung.
- [x] **Slug-Validierung:** Erweiterte Zeichenunterstützung (inkl. `&` und Leerzeichen).
- [x] **Content-Migration:** Trennung von technischem Content (`doc`) und lokalem Content (`main`).
- [x] **Sicherheit:** Security Header (CSP, HSTS Tuning) und gehärtete Password Policy.
- [x] **Rechtssicherheit:** Cookie-Banner Implementierung.
- [ ] **Redirects:** Implementierung eines Alias/Redirect-Systems für umbenannte Slugs.
- [ ] **Admin UI:** Integration einer webbasierten Benutzerverwaltung.
