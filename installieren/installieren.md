# Installations-Anleitung: MeinCMS auf Ubuntu Server 24.04 (mit Unix Sockets)

Diese Anleitung erklärt dir, wie du dein Wiki-System mit maximaler Performance (via Unix Domain Sockets) auf einen Ubuntu-Server installierst.

---

## Teil 1: Das Backup (Deine Daten sichern)

1. Öffne ein Terminal in deinem Projektordner auf deinem Computer.
2. Gib diesen Befehl ein:
   ```bash
   dotnet run --project backup -- export mein_umzug.xml --full
   ```
3. Sichere die Datei `mein_umzug.xml`.
4. dotnet publish --configuration Release --output /home/thorsten/thomas

---

## Teil 2: Vorbereitung auf dem Server

1. **Server aktualisieren:**
   ```bash

   sudo apt update && sudo apt upgrade -y

   ```
2. **.NET 10 installieren:**
   ```bash
   sudo apt install -y dotnet-sdk-10.0
   ```
3. **PostgreSQL installieren & einrichten:**
   ```bash
   sudo apt install -y postgresql postgresql-contrib
   sudo -u postgres psql
   # In psql:
   CREATE DATABASE meincms;
   CREATE USER cmsuser WITH PASSWORD 'DeinSicheresPasswort';
   GRANT ALL PRIVILEGES ON DATABASE meincms TO cmsuser;
   \q
   ```

---

## Teil 3: Installation & Unix Socket Setup

### 1. Code hochladen
Kopiere dein Projekt auf den Server (z.B. nach `/var/www/meincms`).

### 2. Konfiguration anpassen
Bearbeite `mvc/appsettings.json` (Passwort & Socket-Pfad):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=meincms;Username=cmsuser;Password=DeinSicheresPasswort"
  },
  "Kestrel": {
    "UnixSocket": "/run/meincms.sock"
  }
}
```

### 3. Der System-Dienst (Motor)
Wir erstellen den Dienst so, dass er die Socket-Datei nutzen kann.
```bash
sudo nano /etc/systemd/system/meincms.service
```
Inhalt (Pfade anpassen!):
```ini
[Unit]
Description=MeinCMS Wiki System (Unix Socket)
After=network.target postgresql.service

[Service]
WorkingDirectory=/var/www/meincms/mvc
# Wichtig: Wir starten die DLL direkt für bessere Stabilität
ExecStart=/usr/bin/dotnet run --project /var/www/meincms/mvc/mvc.csproj --configuration Release
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=meincms
User=www-data
Group=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
# Verzeichnis für die Socket-Datei vorbereiten
RuntimeDirectory=meincms

[Install]
WantedBy=multi-user.target
```

### 4. Nginx als Schutzschild (Konfiguration für Sockets)
```bash
sudo apt install -y nginx
sudo nano /etc/nginx/sites-available/default
```
Ersetze den Inhalt durch (Domain anpassen!):
```nginx
server {
    listen 80;
    server_name deine-stadt.de;

    location / {
        # Hier verbinden wir Nginx mit dem Unix Socket
        proxy_pass         http://unix:/run/meincms.sock;
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

---

## Teil 4: Start & Import

1. **Dienste aktivieren:**
   ```bash
   sudo systemctl daemon-reload
   sudo systemctl enable meincms
   sudo systemctl start meincms
   sudo systemctl restart nginx
   ```

2. **Daten importieren:**
   ```bash
   cd /var/www/meincms
   dotnet run --project backup -- import mein_umzug.xml
   ```

**FERTIG!** Dein System läuft jetzt über den hochperformanten **Unix Socket**. Das ist die professionellste Art, ASP.NET Core unter Linux zu betreiben.
