---
title: "Produktionsserver einrichten"
date: 2026-04-28
weight: 2
---

# Produktionsserver einrichten

Diese Anleitung beschreibt das Deployment von MeinCMS auf **Ubuntu Server 24.04** mit Nginx als Reverse Proxy über Unix Domain Sockets.

---

## Voraussetzungen auf dem Server

### System aktualisieren

```bash
sudo apt update && sudo apt upgrade -y
```

### .NET 10 Runtime installieren

```bash
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update && sudo apt install -y aspnetcore-runtime-10.0
```

### PostgreSQL installieren

```bash
sudo apt install -y postgresql postgresql-contrib
```

### Nginx installieren

```bash
sudo apt install -y nginx
```

### Certbot (Let's Encrypt) installieren

```bash
sudo apt install -y certbot python3-certbot-nginx
```

---

## Datenbank einrichten

```bash
sudo -u postgres psql
```

```sql
CREATE DATABASE meincmsdb;
CREATE USER meincmsuser WITH PASSWORD 'SicheresProduktionspasswort';
GRANT ALL PRIVILEGES ON DATABASE meincmsdb TO meincmsuser;
\c meincmsdb
GRANT ALL ON SCHEMA public TO meincmsuser;
\q
```

---

## Backup – Export und Import

Das Backup-Tool unterstützt **YAML** und **XML** – die Erkennung erfolgt automatisch anhand der Dateiendung. Gerendertes HTML wird nicht gespeichert (ca. 70 % Größenersparnis) und beim Import automatisch neu erzeugt.

### Export auf dem Entwicklungsrechner

**Alle Mandanten sichern (empfohlen vor jedem Deployment):**

```bash
# YAML-Format
dotnet run --project backup -- export mein_backup.yaml --full

# XML-Format
dotnet run --project backup -- export mein_backup.xml --full
```

**Nur aktuellen Mandanten** (ohne `--full`):

```bash
dotnet run --project backup -- export mein_backup.yaml
dotnet run --project backup -- export mein_backup.xml
```

**Automatischer Dateiname:**

```bash
# Erzeugt z. B.: backup_full_20260428_1430.yaml
dotnet run --project backup -- export --full
```

### Export auf dem Produktionsserver

```bash
cd /var/www/wissen-ahrensburg-de

# XML
sudo ./backup export meine_sicherung.xml --full

# YAML
sudo ./backup export meine_sicherung.yaml --full
```

### Import auf dem Produktionsserver

```bash
cd /var/www/wissen-ahrensburg-de

# Datei direkt im Verzeichnis
sudo ./backup import thomas.xml

# Datei in einem Unterordner
sudo ./backup import thorsten/thomas.xml
```

> Beim Import wird das HTML für jeden Artikel neu generiert. Bei großen Datenbeständen kann das einige Minuten dauern.

### HTML nach Parser-Änderungen neu generieren

```bash
cd /var/www/wissen-ahrensburg-de
sudo ./backup repair
```

---

## Deployment vom Entwicklungsrechner

### 1. Backup exportieren

```bash
dotnet run --project backup -- export meine_sicherung.xml --full
```

### 2. Release-Build erstellen und übertragen

```bash
# Zum Projektverzeichnis navigieren
cd /home/thorsten/wissen-ahrensburg.de

# Release erstellen und veröffentlichen
dotnet publish -c Release -r linux-x64 --self-contained false -o /home/thorsten/publis/wissen-ahrensburg-de/

# Veröffentlichte Dateien zum Remote-Server synchronisieren
rsync -avz --exclude 'bin' --exclude 'obj' --exclude 'config' /home/thorsten/publis/wissen-ahrensburg-de/ tt@ah.city:/var/www/wissen-ahrensburg-de/
```

### 3. Datenbankmigrationen auf dem Server anwenden

```bash
cd /var/www/wissen-ahrensburg-de
dotnet mvc.dll --migrate
```

---

## Einrichtung auf dem Server

### 1. Deploypfad vorbereiten

```bash
sudo mkdir -p /var/www/wissen-ahrensburg-de
sudo chown www-data:www-data /var/www/wissen-ahrensburg-de
```

### 2. Konfiguration anlegen

```bash
sudo nano /var/www/wissen-ahrensburg-de/appsettings.json
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=meincmsdb;Username=meincmsuser;Password=SicheresProduktionspasswort"
  },
  "Tenants": {
    "wissen-ahrensburg.de": "main",
    "doc.wissen-ahrensburg.de": "doc"
  },
  "TenantConfig": {
    "main": { "Title": "wissen-ahrensburg.de", "Logo": "/logo.svg" },
    "doc":  { "Title": "Dokumentation", "Logo": "/logo-doc.svg" }
  },
  "Kestrel": {
    "UnixSocket": "/var/www/wissen-ahrensburg-de/meincms.sock"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "wissen-ahrensburg.de;doc.wissen-ahrensburg.de"
}
```

### 3. Datenbankmigrationen anwenden

```bash
cd /var/www/wissen-ahrensburg-de
dotnet mvc.dll --migrate
```

### 4. Berechtigungen setzen

```bash
sudo chown -R www-data:www-data /var/www/wissen-ahrensburg-de
sudo chmod 750 /var/www/wissen-ahrensburg-de
```

---

## systemd-Service einrichten

```bash
sudo nano /etc/systemd/system/wissen-ahrensburg-de.service
```

```ini
[Unit]
Description=MeinCMS Wiki (wissen-ahrensburg.de)
After=network.target postgresql.service

[Service]
WorkingDirectory=/var/www/wissen-ahrensburg-de
ExecStart=/usr/bin/dotnet /var/www/wissen-ahrensburg-de/mvc.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=meincms
User=www-data
Group=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl daemon-reload
sudo systemctl enable wissen-ahrensburg-de.service
sudo systemctl start wissen-ahrensburg-de.service
sudo systemctl status wissen-ahrensburg-de.service
```

---

## Nginx konfigurieren

```bash
sudo nano /etc/nginx/conf.d/wissen-ahrensburg-de.conf
```

```nginx
# HTTP → HTTPS Weiterleitung
server {
    listen 80;
    listen [::]:80;
    server_name wissen-ahrensburg.de doc.wissen-ahrensburg.de;
    return 301 https://$host$request_uri;
}

# HTTPS
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name wissen-ahrensburg.de doc.wissen-ahrensburg.de;

    ssl_certificate     /etc/letsencrypt/live/wissen-ahrensburg.de/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/wissen-ahrensburg.de/privkey.pem;

    # Sicherheitsempfehlungen
    ssl_protocols       TLSv1.2 TLSv1.3;
    ssl_prefer_server_ciphers on;

    location / {
        proxy_pass         http://unix:/var/www/wissen-ahrensburg-de/meincms.sock;
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

```bash
sudo nginx -t
sudo systemctl reload nginx
```

---

## SSL-Zertifikat ausstellen

```bash
sudo certbot --nginx -d wissen-ahrensburg.de -d doc.wissen-ahrensburg.de
```

Certbot trägt die Zertifikatspfade automatisch in die Nginx-Konfiguration ein.

---

## Ersten Administrator anlegen

```bash
cd /var/www/wissen-ahrensburg-de
sudo ./UserAdmin
```

---

## Updates einspielen

### Auf dem Entwicklungsrechner

```bash
cd /home/thorsten/wissen-ahrensburg.de

dotnet publish -c Release -r linux-x64 --self-contained false -o /home/thorsten/publis/wissen-ahrensburg-de/

rsync -avz --exclude 'bin' --exclude 'obj' --exclude 'config' /home/thorsten/publis/wissen-ahrensburg-de/ tt@ah.city:/var/www/wissen-ahrensburg-de/
```

### Auf dem Server

```bash
cd /var/www/wissen-ahrensburg-de

# Datenbankmigrationen anwenden
dotnet mvc.dll --migrate

# Service neu starten
sudo systemctl restart wissen-ahrensburg-de.service
sudo systemctl status wissen-ahrensburg-de.service
```

---

## Betrieb und Monitoring

### Logs anzeigen

```bash
# Laufende Logs
sudo journalctl -u wissen-ahrensburg-de.service -f

# Logs der letzten Stunde
sudo journalctl -u wissen-ahrensburg-de.service --since "1 hour ago"
```

### Service-Status prüfen

```bash
sudo systemctl status wissen-ahrensburg-de.service
```

---

## Firewall

```bash
sudo ufw allow OpenSSH
sudo ufw allow 'Nginx Full'
sudo ufw enable
sudo ufw status
```

---

## Häufige Probleme

**Socket-Datei existiert beim Start bereits:**

Das ist normal – `Program.cs` löscht eine vorhandene Socket-Datei automatisch beim Start.

**`www-data` hat keinen Schreibzugriff auf den Socket:**

```bash
sudo chown www-data:www-data /var/www/wissen-ahrensburg-de
sudo chmod 750 /var/www/wissen-ahrensburg-de
```

**Nginx-Fehler `connect() to unix:…/meincms.sock failed`:**

```bash
# Prüfen, ob der Service läuft
sudo systemctl status wissen-ahrensburg-de.service
# Socket vorhanden?
ls -la /var/www/wissen-ahrensburg-de/meincms.sock
```

**Zertifikat läuft ab:**

Certbot erneuert automatisch via systemd-Timer. Manuell prüfen:

```bash
sudo certbot renew --dry-run
```
