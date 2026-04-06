# ahrensburg.city (MeinCMS) - Projektübersicht

Dieses Projekt ist ein leichtgewichtiges Content-Management-System (CMS) mit Wiki-ähnlicher Funktionalität und nativer **Multi-Tenancy** (Mandantenfähigkeit), entwickelt mit ASP.NET Core MVC 10.0 und PostgreSQL.

## Status (Stand 06.04.2026)

- **Go-Live bereit:** Das System hat das abschließende Sicherheits-Audit erfolgreich bestanden.
- **Branding:** Hauptmandant wurde auf **ahrensburg.city** umgestellt.
- **Multi-Tenancy:** Vollständig normalisierte Datenbank. Automatische Trennung von technischem Dokumentationsinhalt (`doc`) und Ahrensburg-spezifischen Inhalten (`main`) via Tenant-Migration-Tool durchgeführt.
- **MediaWiki Parser Pro:** Hochentwickelter Compiler-Parser (Tokenizer -> AST -> Serializer) für WikiText. 
  - **Neu:** Unterstützung für echte Absätze (`<p>`), korrekte Listen-Verschachtelung und Inline-HTML-Tags (z.B. `<br>`, `<div>`).
  - Unterstützung für Bold, Italic, Headings, Links, Templates und Kategorien.
- **Markdown Metadaten:** Native Unterstützung für YAML-Frontmatter mit automatischer Extraktion von Kategorien und Filterung der Metadaten aus der HTML-Anzeige.
- **Strukturierte Metadaten:** Native Unterstützung für hierarchische Kategorien und Namensräume (PostgreSQL).
- **Slug-Validierung:** Unterstützung für Unicode, Leerzeichen und Sonderzeichen wie `&`, `:`, `(`, `)`, `!`, `"`, `,`, `` ` ``, und `–`. Dies ermöglicht natürliche Titel wie "Geschichte & Allgemeines".
- **Backup & Repair 2.1:** 
  - Unterstützung für **YAML** und **XML**.
  - **Speichereffizient**: HTML-Inhalt wird beim Export weggelassen und beim Import regeneriert (ca. 70% Ersparnis).
  - **Repair-Modus**: Tool zur automatischen Reparatur und Regeneration aller HTML-Inhalte in der Datenbank nach Parser-Updates.
- **Deployment:** Unterstützung für **Unix Domain Sockets** (`/run/meincms.sock`) für hochperformanten Betrieb hinter Nginx.
- **Sicherheit:** Integration von `HtmlSanitizer`, gehärtete Identity-Policies, CSRF-Schutz und strikte Content-Security-Policy (CSP).

## Architektur

- **`mvc/`**: Die Haupt-Webanwendung (Wiki-only).
  - **Multi-Tenancy**: Dynamische Mandantenerkennung via Hostname und **Request-spezifische Filterung** im `ApplicationDbContext`.
  - **Parser**: Parsoid-ähnlicher MediaWiki-Parser in `mvc/Parser/`.
  - **Models**: `WikiArtikel`, `WikiArtikelVersion`, `WikiNamespace`, `WikiCategory`.
- **`Services/`**: Geschäftslogik, `PageService` (mit Markdown & WikiText Support), `TenantService`.
- **`UserAdmin/`**: Konsolenanwendung für die Verwaltung der Administratoren.
- **`backup/`**: Spezialisiertes Tool für YAML/XML-Export, Import und Datenbank-Reparatur.

## Technologien

- **Framework**: .NET 10.0 (ASP.NET Core)
- **Datenbank**: PostgreSQL (via Npgsql)
- **Bibliotheken**: Markdig, HtmlSanitizer, DiffPlex, YamlDotNet.

## Erstellen und Ausführen

- **Web**: `dotnet run --project mvc`
- **Admin**: `dotnet run --project UserAdmin`
- **Backup Export**: `dotnet run --project backup -- export meine_daten.yaml --full`
- **Backup Import**: `dotnet run --project backup -- import meine_daten.yaml`
- **Repair HTML**: `dotnet run --project backup -- repair`

## Entwicklungskonventionen

- **Multi-Tenancy**: Jede Abfrage wird automatisch nach dem aktuellen Mandanten gefiltert. Die `TenantId` wird dynamisch pro Request ermittelt.
- **Normalisierung**: Standard-Mandant ist `"main"`. Technischer Content wird dem Mandanten `"doc"` zugeordnet.
- **Sicherheit**: Alle Markdown-Inhalte werden vor der Anzeige via `HtmlSanitizer` bereigt. CSRF-Schutz ist global aktiviert.

## TODO / Roadmap

- [x] **Multi-Tenancy:** Native Mandantenfähigkeit via Hostname-Erkennung.
- [x] **Slug-Validierung:** Erweiterte Zeichenunterstützung (inkl. `&` und Leerzeichen).
- [x] **Content-Migration:** Trennung von technischem Content (`doc`) und lokalem Content (`main`).
- [x] **MediaWiki Support:** Compiler-basierter Parser für WikiText.
- [x] **Strukturierte Kategorien:** Datenbankmodelle für Namespaces und hierarchische Kategorien.
- [x] **Sicherheit:** Security Header (CSP, HSTS Tuning) und gehärtete Password Policy.
- [x] **Rechtssicherheit:** Cookie-Banner Implementierung.
- [x] **HTML Repair:** Tool zur Regeneration der Datenbank-Inhalte nach Parser-Fixes.
- [ ] **Redirects:** Implementierung eines Alias/Redirect-Systems für umbenannte Slugs.
- [ ] **Admin UI:** Integration einer webbasierten Benutzerverwaltung.
