# MeinCMS

MeinCMS ist ein leichtgewichtiges Content-Management-System (CMS) mit Wiki-Funktionalität und nativer **Multi-Tenancy** (Mandantenfähigkeit), entwickelt mit **ASP.NET Core MVC 10.0** und **PostgreSQL**. Das System ermöglicht den Betrieb mehrerer Domains auf einer einzigen Instanz bei strikter Datentrennung.

## 🌟 Hauptmerkmale

*   **Multi-Tenancy**: Betreiben Sie `domain-a.tld` und `domain-b.tld` mit unterschiedlichen Inhalten auf derselben Anwendung.
*   **Sichere Datentrennung**: Automatische Filterung aller Datenbankabfragen via Global Query Filters.
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
*Passen Sie den Connection-String und das **Tenants-Mapping** in `mvc/appsettings.json` an:*

```json
"Tenants": {
  "meine-domain.de": "main",
  "doc.meine-domain.de": "doc"
}
```

### 2. Datenbank-Migrationen anwenden
```bash
dotnet ef database update --project mvc
```

## 💻 Programm starten

### Web-Anwendung (Wiki)
```bash
dotnet run --project mvc
```
Die Anwendung erkennt den Mandanten automatisch anhand des aufgerufenen Hostnamens.

### Benutzeradministration (UserAdmin)
```bash
dotnet run --project UserAdmin
```

## 🛡️ Sicherheit & Architektur

MeinCMS wurde mehreren Sicherheits-Audits unterzogen (Stand 24. März 2026):

- **Mandantentrennung**: Technisch erzwungene Trennung auf Datenbankebene (Global Query Filters).
- **XSS-Schutz**: Alle Wiki-Inhalte werden via `HtmlSanitizer` bereinigt.
- **Identity**: Gehärtete Password-Policy (min. 12 Zeichen) und Account-Lockout.
- **Angriffsfläche**: `HomeController` entfernt, Wiki-only Routing.

## 🗺️ Roadmap

- [x] **Multi-Tenancy**: Flexible Mandantenfähigkeit via Konfiguration.
- [x] **Versionskontrolle**: Diff-Ansicht für Artikelversionen.
- [ ] **Testing**: Aufbau einer umfassenden Testsuite mit xUnit.
- [ ] **Themes**: Unterstützung für mandantenspezifische CSS-Anpassungen.

## 🚀 Deployment auf Ubuntu (systemd)

Um die Anwendung professionell im Hintergrund zu betreiben und nach einem Neustart automatisch zu starten, nutzen Sie **systemd**.

### 1. Service-Datei erstellen
Erstellen Sie die Datei `/etc/systemd/system/meincms.service`:

```ini
[Unit]
Description=MeinCMS Wiki Application
After=network.target postgresql.service

[Service]
WorkingDirectory=/var/www/meincms/mvc
ExecStart=/usr/bin/dotnet /var/www/meincms/mvc/mvc.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=meincms
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

### 2. Dienst verwalten
```bash
sudo systemctl daemon-reload
sudo systemctl enable meincms.service
sudo systemctl start meincms.service
```

### 3. Nginx als Reverse Proxy (Wichtig!)
Stellen Sie sicher, dass Nginx den Host-Header für die Mandantenfähigkeit weitergibt:

```nginx
location / {
    proxy_pass http://localhost:5000;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
}
```

## 🏗 Projektstruktur

- `mvc/`: Die ASP.NET Core MVC Webanwendung (Wiki).
- `Services/`: Gemeinsame Geschäftslogik (Wiki-Dienste, Mandanten-Dienst, Blogs).
- `UserAdmin/`: Konsolenanwendung für administrative Zwecke.
- `bericht/`: Detaillierte Sicherheits-Audit-Berichte.

## 🚀 Deployment

Verwenden Sie `dotnet publish` für die Vorbereitung des Produktivbetriebs. Ein Betrieb hinter einem Reverse Proxy (**Nginx**) mit korrekt gesetztem `Host`-Header ist für die Mandantenfähigkeit zwingend erforderlich.
