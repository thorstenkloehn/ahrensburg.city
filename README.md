# MeinCMS

MeinCMS ist ein leichtgewichtiges Content-Management-System (CMS) mit Wiki-Funktionalität, entwickelt mit **ASP.NET Core MVC 10.0** und **PostgreSQL**.

## 🛠 Voraussetzungen

Bevor Sie das Projekt starten, müssen folgende Komponenten auf Ihrem System installiert sein:

*   **[.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)**
*   **[PostgreSQL](https://www.postgresql.org/)** (lokal oder als Docker-Container)
*   **EF Core CLI Tools**: Falls noch nicht installiert, führen Sie diesen Befehl aus:
    ```bash
    dotnet tool install --global dotnet-ef
    ```

## 🚀 Installation und Einrichtung

### 1. Konfiguration vorbereiten
Kopieren Sie die Vorlagendateien für die Konfiguration an ihre tatsächlichen Speicherorte:

```bash
cp mvc/_appsettings.Development.json mvc/appsettings.Development.json
cp mvc/_appsettings.json mvc/appsettings.json
```

*Hinweis: Passen Sie den Connection-String in `mvc/appsettings.json` an Ihre PostgreSQL-Instanz an.*

### 2. Datenbank-Setup (Linux/PostgreSQL)
Erstellen Sie die Datenbank und vergeben Sie die notwendigen Rechte für Ihren Benutzer (Beispiel für den Benutzer `thorsten`):

```bash
sudo -u postgres -i
createdb -E UTF8 -O thorsten mvc
psql -d mvc -c "GRANT ALL PRIVILEGES ON DATABASE mvc TO thorsten"
exit
```

### 3. Datenbank-Migrationen anwenden
Führen Sie die Migrationen vom Projekt-Root aus, um das Tabellenschema zu erstellen:

```bash
dotnet ef database update --project mvc
```

## 💻 Programm starten

### Web-Anwendung (Wiki)
Starten Sie die Hauptanwendung aus dem Root-Verzeichnis:

```bash
dotnet run --project mvc
```
Die Anwendung ist standardmäßig unter `http://localhost:5000` (oder dem in `launchSettings.json` definierten Port) erreichbar.

### Benutzeradministration (UserAdmin)
Für administrative Aufgaben (Benutzer erstellen/auflisten) nutzen Sie das Konsolentool:

```bash
dotnet run --project UserAdmin
```

## 🛡️ Schwachstellen (Security Assessment)

Aktuell identifizierte Sicherheitsrisiken, die in kommenden Updates adressiert werden:

1.  **Stored XSS (Cross-Site Scripting)**: Wiki-Inhalte werden als HTML gerendert (`@Html.Raw`). Ohne HTML-Sanitizer (z. B. `HtmlSanitizer`) können Angreifer schädliche Skripte einbetten.
2.  **Unvollständige Auth-Middleware**: In `Program.cs` fehlt `app.UseAuthentication()`, wodurch die Identität in der Pipeline nicht korrekt verarbeitet wird.
3.  **Fehlendes RBAC**: Es gibt keine differenzierten Rollen (Admin/Editor). Jeder angemeldete User hat volle Schreibrechte.
4.  **Konfigurations-Sicherheit**: Sensible Daten wie Connection Strings sollten aus `appsettings.json` in Umgebungsvariablen oder Secrets verschoben werden.
5.  **Schwache Passwörter**: Das `UserAdmin`-Tool validiert Passwortstärken aktuell nur minimal.

## 🚩 Meilensteine

- [x] **M1: Fundament & Daten**: Projekt-Setup mit .NET 10, PostgreSQL & EF Core.
- [x] **M2: Wiki-Kern**: Dynamisches Routing mit `/`-Support, Markdown-Rendering & Versionierung.
- [ ] **M3: Sicherheit & Identity**: Vollständige Auth-Integration & XSS-Schutz (In Arbeit).
- [ ] **M4: Medien & Suche**: Dateimanagement und Volltextsuche (Geplant).

## 🗺️ Roadmap

- [ ] **Sicherheit**:
    - [ ] Integration von `HtmlSanitizer` zur Absicherung der HTML-Ausgabe.
    - [ ] Korrektur der Middleware-Pipeline (`UseAuthentication`).
    - [ ] Einführung von Rollen (Admin, Editor, Viewer).
- [ ] **Funktionen**:
    - [ ] **Dateiverwaltung**: Upload-System für Bilder und Dokumente.
    - [ ] **Editor-Upgrade**: Integration eines WYSIWYG- oder verbesserten Markdown-Editors (z. B. EasyMDE).
    - [ ] **Suche**: Implementierung einer Volltextsuche über alle Artikel.
    - [ ] **API**: REST-Schnittstelle für Wiki-Inhalte.
- [ ] **Infrastruktur**:
    - [ ] **Testing**: Aufbau einer Testsuite mit xUnit (Unit- & Integrationstests).
    - [ ] **Docker**: Bereitstellung von Docker-Images für die Anwendung.
    - [ ] **CI/CD**: Automatisierte Builds und Tests via GitHub Actions.

## 🏗 Projektstruktur

- `mvc/`: Die ASP.NET Core MVC Webanwendung.
- `Services/`: Gemeinsame Geschäftslogik und Dienste.
- `UserAdmin/`: Konsolenanwendung für administrative Zwecke.
- `Module.Lernplattform/`: Ein (in Entwicklung befindliches) Modul für Lerninhalte.

## 🚀 Deployment (Bereitstellung)

Um die Anwendung für einen Produkt-Server vorzubereiten, verwenden Sie den Befehl `dotnet publish`. Dieser kompiliert den Code im Release-Modus und sammelt alle notwendigen Dateien in einem Zielverzeichnis.

### Webanwendung (mvc) veröffentlichen

Führen Sie den Befehl im Hauptverzeichnis des Projekts aus:

```bash
dotnet publish mvc/mvc.csproj -c Release -o ./publish
```

**Erklärung der Parameter:**
- `-c Release`: Optimiert die Anwendung für maximale Performance.
- `-o ./publish`: Speichert die fertigen Dateien im Ordner `publish`.

### Auf dem Server übertragen

Verwenden Sie `rsync`, um den Inhalt des `publish`-Ordners effizient auf Ihren Server zu kopieren:

```bash
rsync -avz ./publish/ benutzer@ihr-server:/pfad/zum/zielverzeichnis/
```

*Erklärung der Flags:*
- `-a` (archive): Behält Berechtigungen und Zeitstempel bei.
- `-v` (verbose): Zeigt den Fortschritt an.
- `-z` (compress): Komprimiert die Daten während der Übertragung.

### Auf dem Server ausführen

1. Stellen Sie sicher, dass die `appsettings.json` auf dem Server die korrekten PostgreSQL-Verbindungsdaten enthält.
2. Starten Sie die Anwendung im Zielverzeichnis auf dem Server:
   ```bash
   dotnet mvc.dll
   ```

### Automatischer Start mit systemd

Damit die Anwendung auch nach einem Server-Neustart automatisch läuft und bei Abstürzen neu startet, sollte ein `systemd`-Service eingerichtet werden.

1. **Service-Datei erstellen**:
   Erstellen Sie eine Datei unter `/etc/systemd/system/meincms.service`:
   ```ini
   [Unit]
   Description=MeinCMS Webanwendung
   After=network.target postgresql.service

   [Service]
   WorkingDirectory=/pfad/zum/zielverzeichnis
   ExecStart=/usr/bin/dotnet /pfad/zum/zielverzeichnis/mvc.dll
   Restart=always
   # Restart service after 10 seconds if the dotnet service crashes:
   RestartSec=10
   KillSignal=SIGINT
   SyslogIdentifier=meincms
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production
   Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

   [Install]
   WantedBy=multi-user.target
   ```

2. **Service aktivieren und starten**:
   ```bash
   sudo systemctl daemon-reload
   sudo systemctl enable meincms.service
   sudo systemctl start meincms.service
   ```

### Reverse Proxy mit Nginx einrichten

Um die Anwendung über Port 80 (HTTP) oder 443 (HTTPS) erreichbar zu machen und Sicherheitsfeatures von Nginx zu nutzen, sollte Nginx als Reverse Proxy vor die .NET-Anwendung geschaltet werden.

1. **Nginx installieren** (falls noch nicht geschehen):
   ```bash
   sudo apt update
   sudo apt install nginx
   ```

2. **Konfigurationsdatei erstellen**:
   Erstellen Sie eine Datei unter `/etc/nginx/sites-available/meincms`:
   ```nginx
   server {
       listen 80;
       server_name ihr-domainname.de; # Hier Ihre Domain oder IP eintragen

       location / {
           proxy_pass         http://localhost:5000; # Port aus launchSettings.json / Program.cs
           proxy_http_version 1.1;
           proxy_set_header   Upgrade $http_upgrade;
           proxy_set_header   Connection keep-alive;
           proxy_set_header   Host $host;
           proxy_cache_bypass $http_upgrade;
           proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
           proxy_set_header   X-Forwarded-Proto $scheme;
       }
   }
   ```

3. **Konfiguration aktivieren und Nginx neu starten**:
   ```bash
   sudo ln -s /etc/nginx/sites-available/meincms /etc/nginx/sites-enabled/
   sudo nginx -t # Testet die Konfiguration auf Fehler
   sudo systemctl restart nginx
   ```

### Wichtiger Hinweis zu HTTPS (SSL)
Es wird dringend empfohlen, ein SSL-Zertifikat mit **Certbot (Let's Encrypt)** zu installieren, sobald die Domain auf den Server zeigt:
```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d ihr-domainname.de
```


> **Sicherheitshinweis:** Das Tool `UserAdmin` sollte **nicht** auf dem öffentlichen Web-Server dauerhaft installiert werden. Nutzen Sie es nur lokal oder bei manuellem Bedarf für administrative Aufgaben und löschen Sie es danach wieder vom Server.
