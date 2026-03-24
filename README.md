# MeinCMS

MeinCMS ist ein leichtgewichtiges Content-Management-System (CMS) mit Wiki-Funktionalität, entwickelt mit **ASP.NET Core MVC 10.0** und **PostgreSQL**. Das System ist für maximale Sicherheit und Einfachheit optimiert.

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
*Passen Sie den Connection-String in `mvc/appsettings.json` an.*

### 2. Datenbank-Migrationen anwenden
```bash
dotnet ef database update --project mvc
```

## 💻 Programm starten

### Web-Anwendung (Wiki)
```bash
dotnet run --project mvc
```
Die Anwendung ist unter `http://localhost:5000` erreichbar.

### Benutzeradministration (UserAdmin)
```bash
dotnet run --project UserAdmin
```

## 🛡️ Sicherheit & Architektur

MeinCMS wurde mehreren Sicherheits-Audits unterzogen (Stand März 2026):

- **XSS-Schutz**: Alle Wiki-Inhalte werden via `HtmlSanitizer` bereinigt.
- **Identity**: Gehärtete Password-Policy (min. 12 Zeichen) und Account-Lockout.
- **Angriffsfläche**: `HomeController` entfernt, Wiki-only Routing.
- **Keine Uploads**: Datei-Uploads sind explizit nicht implementiert, um RCE-Risiken zu eliminieren.
- **Administration**: Benutzerverwaltung erfolgt offline/lokal via CLI-Tool.

## 🚩 Meilensteine

- [x] **M1: Fundament & Daten**: Projekt-Setup mit .NET 10 & PostgreSQL.
- [x] **M2: Wiki-Kern**: Dynamisches Routing & Versionierung.
- [x] **M3: Sicherheit & Identity**: Auth-Integration, XSS-Schutz & Password Policy.
- [x] **M4: Versionskontrolle**: Diff-Ansicht für Artikelversionen.
- [ ] **M5: Qualitätssicherung**: Unit- & Integrationstests (In Arbeit).

## 🗺️ Roadmap

- [ ] **Testing**: Aufbau einer umfassenden Testsuite mit xUnit.
- [ ] **Performance**: Monitoring der Diff-Ansicht bei sehr großen Artikel-Historien.
- [ ] **UI/UX**: Unterstützung für Themes und CSS-Customization.
- [ ] **Deployment**: Docker-Images und CI/CD Pipelines.

## 🏗 Projektstruktur

- `mvc/`: Die ASP.NET Core MVC Webanwendung (Wiki).
- `Services/`: Gemeinsame Geschäftslogik (Wiki-Dienste, Blogs).
- `UserAdmin/`: Konsolenanwendung für administrative Zwecke.
- `bericht/`: Detaillierte Sicherheits-Audit-Berichte.

## 🚀 Deployment

Verwenden Sie `dotnet publish` für die Vorbereitung des Produktivbetriebs. Ein Betrieb hinter einem Reverse Proxy (Nginx/Apache) mit HTTPS ist zwingend erforderlich.

```bash
dotnet publish mvc/mvc.csproj -c Release -o ./publish
```

Detaillierte Anweisungen zur Einrichtung von `systemd` und `Nginx` finden Sie in der internen Dokumentation.
