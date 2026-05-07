## Vorbereitung

### Entwicklung Rechner

Führe folgende Befehle aus, um die Anwendung zu exportieren, zu veröffentlichen und auf den Server zu übertragen:

```bash
dotnet run --project backup -- export mein_umzug.xml --full
dotnet publish -c Release -r linux-x64 --self-contained false -o /home/thorsten/publis/wiki-ahrensburg-de/
dotnet ef migrations script -o migration.sql
scp migration.sql thorsten@ttt.de:/var/www
rsync -avz --exclude 'bin' --exclude 'obj' /home/thorsten/publis/wiki-ahrensburg-de/ tt@ah.city:/var/www/wiki-ahrensburg-de/
```

### Server Datenbank

Führe die SQL-Migrationen aus der Datei `migration.sql` aus:

```bash
psql -h localhost -U dein_db_user -d deine_datenbank_name -f /tmp/migration.sql
```

### Systemd Service

Erstelle die Service-Datei:

```bash
sudo nano /etc/systemd/system/wiki-ahrensburg-de.service
```

Füge folgenden Inhalt ein:

```ini
[Unit]
Description=MeinCMS Wiki System (Unix Socket)
After=network.target postgresql.service

[Service]
WorkingDirectory=/var/www/wiki-ahrensburg-de/
ExecStart=/usr/bin/dotnet /var/www/wiki-ahrensburg-de/mvc.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=meincms
User=www-data
Group=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
RuntimeDirectory=meincms

[Install]
WantedBy=multi-user.target
```

Aktiviere und starte den Service:

```bash
sudo systemctl daemon-reload
sudo systemctl enable wiki-ahrensburg-de.service
sudo systemctl start wiki-ahrensburg-de.service
sudo systemctl status wiki-ahrensburg-de.service
```

### Nginx Konfiguration

Bearbeite die Nginx-Konfiguration:

```bash
sudo nano /etc/nginx/conf.d/start.conf
```

Füge folgenden Server-Block ein:

```nginx
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name doc.wiki-ahrensburg.de wiki-ahrensburg.de;
    ssl_certificate /etc/letsencrypt/live/wiki-ahrensburg.de/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/wiki-ahrensburg.de/privkey.pem;

    location / {
        proxy_pass         http://unix:/var/www/wiki-ahrensburg-de/meincms.sock;
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

