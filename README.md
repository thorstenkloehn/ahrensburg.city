# ahrensburg.city (MeinCMS)

**ahrensburg.city** ist ein leichtgewichtiges Content-Management-System (CMS) mit Wiki-Funktionalität und nativer **Multi-Tenancy** (Mandantenfähigkeit), entwickelt mit **ASP.NET Core MVC 10.0** und **PostgreSQL**. Das System ermöglicht den Betrieb mehrerer Domains auf einer einzigen Instanz bei strikter Datentrennung.

## 🌟 Hauptmerkmale

*   **Multi-Tenancy**: Betreiben Sie `ahrensburg.city` (Hauptmandant) und `doc.ahrensburg.city` (Dokumentation) auf derselben Anwendung.
*   **Sichere Datentrennung**: Automatische Filterung aller Datenbankabfragen via dynamischer Global Query Filters pro Request.
*   **Rechtssicherheit**: Integrierter, konfigurierbarer Cookie-Banner für DSGVO-Konformität.
*   **Wiki-Kern**: Dynamisches Routing, Markdown-Unterstützung und YAML-Metadaten.
*   **Versionierung**: Lückenlose Historie aller Änderungen mit Diff-Ansicht für Administratoren.
*   **Security by Design**: Kein Datei-Upload, HtmlSanitizer für Markdown, gehärtete Identity-Policies.

## 🛠 Voraussetzungen

Bevor Sie das Projekt starten, müssen folgende Komponenten auf Ihrem System installiert sein:

*   **[.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)**
*   **[PostgreSQL](https://www.postgresql.org/)**
*   **EF Core CLI Tools**:
    ```bash
    dotnet tool install --global dotnet-ef
    ```

## 🚀 Installation und Einrichtung

### 1. Konfiguration vorbereiten
```bash
cp mvc/_appsettings.json mvc/appsettings.json
```
*Passen Sie den Connection-String und das **Tenants-Mapping** in `mvc/appsettings.json` an.*

### 2. Datenbank-Migrationen anwenden
```bash
dotnet ef database update --project mvc
```

## 💻 Programm starten

Detaillierte Anweisungen zum Starten der Anwendung in verschiedenen Umgebungen (Entwicklung mit Port 5000 oder Produktion mit Unix Sockets) finden Sie in der separaten Anleitung:

👉 **[Anleitung zum Starten (Entwicklung & Produktion)](mvc/Anleitung_Start.md)**

### Web-Anwendung (Wiki) - Entwicklung
```bash
dotnet run --project mvc --launch-profile http
```

### Benutzeradministration (UserAdmin)
```bash
dotnet run --project UserAdmin
```

### Backup & Inhalts-Migration (backup)
Das spezialisierte Backup-Tool ermöglicht den Export und Import von Wiki-Inhalten (ohne Benutzerdaten) über alle Mandanten hinweg.

#### 1. Backup/Export (Inhalte sichern)
```bash
# Exportiert alle Mandanten in eine XML-Datei (kompakt ohne HTML)
dotnet run --project backup -- export meine_sicherung.xml --full

# NEU: Export im kompakten YAML-Format
dotnet run --project backup -- export meine_sicherung.yaml --full
```
*Das Tool normalisiert automatisch leere `TenantId`-Felder zu `"main"`, um Datenverlust durch Filter zu vermeiden.*

#### 2. Import (Inhalte einspielen/aktualisieren)
```bash
dotnet run --project backup -- import meine_sicherung.xml
```
*Nutzt eine intelligente Upsert-Logik: Existierende Artikel werden um neue Versionen ergänzt, fehlende Artikel werden neu angelegt. Fehlender HTML-Inhalt wird automatisch aus dem Markdown regeneriert.*

### 🗄️ PostgreSQL Datenbank-Backup

Für die Sicherung der Datenbank auf SQL-Ebene (z. B. für Migrationen oder Snapshots) können die Standard-PostgreSQL-Tools verwendet werden.

#### 1. Vollständiges Backup (inkl. Benutzerdaten)
Sichert die gesamte Datenbank inklusive aller Wiki-Artikel, Versionen und Benutzeraccounts (Identity-Tabellen).
```bash
pg_dump -U [DEIN_DB_USER] -d [DB_NAME] > meincms_vollbackup.sql
```

#### 2. Inhalts-Backup (nur Wiki-Inhalte, KEINE Benutzerdaten)
Sichert alle Mandanten-Inhalte (Artikel und Historie), schließt aber alle Identity-Tabellen (Benutzer, Rollen, Passwörter) aus.
```bash
pg_dump -U [DEIN_DB_USER] -d [DB_NAME] -T 'AspNet*' > meincms_inhalte_nur.sql
```
*(Das `-T 'AspNet*'` Flag schließt alle Tabellen aus, die mit `AspNet` beginnen, was dem Standard von ASP.NET Core Identity entspricht.)*

#### 3. Wiederherstellung (Restore)
Um ein Backup wieder einzuspielen, muss die Zieldatenbank vorhanden (und idealerweise leer) sein.

**Schritt A: Datenbank erstellen (falls nicht vorhanden)**
```bash
createdb -U [DEIN_DB_USER] [DB_NAME]
```

**Schritt B: SQL-Dump einspielen**
```bash
psql -U [DEIN_DB_USER] -d [DB_NAME] -f meincms_vollbackup.sql
```
*(Bei einem Inhalts-Backup ohne Benutzerdaten stellen Sie sicher, dass die Migrationen zuvor via `dotnet ef database update` ausgeführt wurden, damit das Schema vorhanden ist.)*

## 🛡️ Sicherheit & Architektur

MeinCMS wurde mehreren Sicherheits-Audits unterzogen (Stand 24. März 2026):

- **Mandantentrennung**: Technisch erzwungene Trennung auf Datenbankebene (Global Query Filters).
- **XSS-Schutz**: Alle Wiki-Inhalte werden via `HtmlSanitizer` bereinigt.
- **Identity**: Gehärtete Password-Policy (min. 12 Zeichen) und Account-Lockout.
- **Angriffsfläche**: `HomeController` entfernt, Wiki-only Routing.

## 🗺️ Roadmap

- [x] **Multi-Tenancy**: Flexible Mandantenfähigkeit via Konfiguration.
- [x] **Versionskontrolle**: Diff-Ansicht für Artikelversionen.
- [x] **Testing**: Umfassende Testsuite mit xUnit (Unit- & Integrationstests).
- [x] **Themes**: Unterstützung für mandantenspezifische CSS-Anpassungen.
- [x] **Unix Sockets**: Hochperformantes Deployment-Setup.

## 🏗 Projektstruktur

- `mvc/`: Die ASP.NET Core MVC Webanwendung (Wiki).
- `Services/`: Gemeinsame Geschäftslogik (Wiki-Dienste, Mandanten-Dienst, Blogs).
- `UserAdmin/`: Konsolenanwendung für die Benutzerverwaltung.
- `backup/`: Spezialisiertes Tool für XML-Export und -Import von Wiki-Inhalten.
- `bericht/`: Detaillierte Sicherheits-Audit-Berichte.
- `docs/`: Statisch generierte Projektdokumentation (DocFX).

## 🚀 Deployment

Verwenden Sie `dotnet publish` für die Vorbereitung des Produktivbetriebs. Detaillierte Informationen zum professionellen Deployment (systemd, Nginx, Unix Sockets) finden Sie in der **[Anleitung zum Starten](mvc/Anleitung_Start.md)**.
