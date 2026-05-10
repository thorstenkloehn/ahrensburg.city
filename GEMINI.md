# wissen-ahrensburg.de (MeinCMS) - Projektübersicht

Dieses Projekt ist ein leichtgewichtiges Content-Management-System (CMS) mit Wiki-ähnlicher Funktionalität und nativer **Multi-Tenancy** (Mandantenfähigkeit), entwickelt mit ASP.NET Core MVC 10.0 und PostgreSQL.

## Status (Stand 10.05.2026)

- **Wartung & Hygiene:** 
  - Alle NuGet-Sicherheitsanfälligkeiten (NU1901, NU1903) behoben durch Updates auf sichere Versionen (z.B. `System.Security.Cryptography.Xml` 10.0.7).
  - C#-Naming-Warnings (CS8981) in den Migrationen durch Umbenennung behoben.
  - Bereinigung der Build-Warnungen für einen "Green Build".
- **Editor-Stabilität:** Kritische Fixes für die Syntax-Umschaltung (Markdown/MediaWiki) implementiert.
  - **Bidirektionale Konvertierung:** Automatischer Transfer und Übersetzung von Inhalten beim Wechsel der Syntax im Editor.
  - **Robustes Toggling:** Verwendung von `style.display` und `DOMContentLoaded`-Event-Listenern für garantierte UI-Synchronisation.
- **Razor-Kompatibilität:** Korrekte Verwendung von `<!option>` Tags zur Vermeidung von Tag-Helper-Konflikten (RZ1031).
- **Standard-Syntax:** MediaWiki ist nun der Standard-Typ für alle neuen Artikel.
- **Go-Live bereit:** Das System hat das abschließende Sicherheits-Audit erfolgreich bestanden.
- **Sicherheit & Hardening:** 
  - Integration von `HtmlSanitizer`.
  - Gehärtete Identity-Policies (inkl. Account-Lockout).
  - CSRF-Schutz und strikte Content-Security-Policy (CSP).
  - **Neu:** Vollständige Entfernung von Inline-Skripten zugunsten externer Dateien (`site.js`) zur Einhaltung strenger CSP-Richtlinien.
- **MediaWiki Parser Pro:** Hochentwickelter Compiler-Parser (Tokenizer -> AST -> Serializer) für WikiText. 
  - **Optimiert:** Verbesserte Textverarbeitung und stabilere Absatz-Generierung.
  - Unterstützung für echte Absätze (`<p>`), korrekte Listen-Verschachtelung und Inline-HTML-Tags (z.B. `<br>`, `<div>`).
- **Benutzerfreundlichkeit:** Verbesserte Syntaxauswahl und Inhaltsbestimmung im Editor-Formular.
- **Dokumentation:** 
  - Neue Anleitungen für den **Produktions-Server Setup** und **Daten-Sicherung via SSH/SCP** hinzugefügt (`Anleitung/`).
- **Streamlining:** Alle Kartendienste (Leaflet, GeoJSON, POI-Controller) wurden vollständig entfernt, um das System zu entschlacken.
- **Lizenz:** Das gesamte Projekt steht unter der **GNU Affero General Public License v3.0 (AGPL-3.0)**.
- **Branding:** Hauptmandant ist **wissen-ahrensburg.de**.
- **Multi-Tenancy:** Vollständig normalisierte Datenbank mit automatischer Trennung von technischem Dokumentationsinhalt (`doc`) und Stadt-spezifischen Inhalten (`main`).
- **Suche:** Performante **Volltextsuche** (Full-Text Search) in Wiki-Artikeln.
- **Markdown Metadaten:** Native Unterstützung für YAML-Frontmatter.
- **Slug-Validierung:** Unterstützung für Unicode, Leerzeichen und Sonderzeichen (`&`, `:`, `(`, `)`, `!`, `"`, `,`, `` ` ``, `–`).
- **Backup & Repair 2.1:** Unterstützung für YAML/XML, speichereffizienter Export und automatischer Repair-Modus zur HTML-Regeneration.
- **Deployment:** Unterstützung für **Unix Domain Sockets** (`/run/meincms.sock`) für Nginx-Betrieb.

## Architektur

- **`mvc/`**: Die Haupt-Webanwendung (Wiki-only).
  - **Multi-Tenancy**: Dynamische Mandantenerkennung via Hostname und **Request-spezifische Filterung** im `ApplicationDbContext`.
  - **Parser**: Parsoid-ähnlicher MediaWiki-Parser in `mvc/Parser/`.
  - **Models**: `WikiArtikel`, `WikiArtikelVersion`, `WikiNamespace`, `WikiCategory`.
- **`Services/`**: Geschäftslogik, `PageService` (mit Markdown & WikiText Support), `TenantService`.
- **`UserAdmin/`**: Konsolenanwendung für die Verwaltung der Administratoren.
- **`backup/`**: Spezialisiertes Tool für YAML/XML-Export, Import und Datenbank-Reparatur.

## Lizenz

Dieses Projekt ist unter der **GNU Affero General Public License v3.0 (AGPL-3.0)** lizenziert. Weitere Details finden Sie in der [LICENSE](LICENSE) Datei.

## Technologien

- **Framework**: .NET 10.0 (ASP.NET Core)
- **Datenbank**: PostgreSQL (via Npgsql)
- **Parser**: Eigene Compiler-basierte Implementierungen für **WikiText** und **Markdown** (keine externen Abhängigkeiten wie Markdig).
- **Bibliotheken**: HtmlSanitizer, DiffPlex, YamlDotNet.

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

### Wichtige Fehlervermeidung (Lessons Learned)

- **Razor-Tags:** Bei `<option>`-Tags in Razor-Views, die C#-Attribute (z.B. `@(selected)`) verwenden, muss zwingend das Ausrufezeichen-Präfix `<!option>` genutzt werden, um den `RZ1031`-Fehler zu vermeiden.
- **Editor-Sichtbarkeit:** Sichtbarkeits-Toggling im Editor darf sich nicht nur auf CSS-Klassen (`d-none`) verlassen. Zusätzlich muss `style.display` direkt via JavaScript gesetzt werden, um CSS-Spezifitäts-Konflikte zu vermeiden.
- **Syntax-Eindeutigkeit:** Der Server (`PageController`) muss beim Speichern explizit prüfen, welches Inhaltsfeld (Markdown vs. MediaWiki) zur gewählten Syntax passt, und das jeweils andere Feld leeren, um Inkonsistenzen in der Datenbank zu verhindern.
- **JS-Initialisierung:** Skripte wie `editor.js` müssen ihre Initialisierung (z.B. `toggleSyntax()`) sowohl über `DOMContentLoaded` als auch über explizite Event-Listener absichern.

## TODO / Roadmap

- [x] **Multi-Tenancy:** Native Mandantenfähigkeit via Hostname-Erkennung.
- [x] **Volltextsuche:** Suche über alle Artikelinhalte.
- [x] **Slug-Validierung:** Erweiterte Zeichenunterstützung (inkl. `&` und Leerzeichen).
- [x] **Content-Migration:** Trennung von technischem Content (`doc`) und lokalem Content (`main`).
- [x] **MediaWiki Support:** Compiler-basierter Parser für WikiText.
- [x] **Sicherheit:** Account-Lockout und gehärtete Password Policy.
- [x] **Rechtssicherheit:** Cookie-Banner Implementierung.
- [x] **HTML Repair:** Tool zur Regeneration der Datenbank-Inhalte nach Parser-Fixes.
- [ ] **Redirects:** Implementierung eines Alias/Redirect-Systems für umbenannte Slugs.
- [ ] **Admin UI:** Integration einer webbasierten Benutzerverwaltung.
