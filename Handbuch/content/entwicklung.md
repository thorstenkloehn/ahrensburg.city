---
title: "Entwicklungsrechner einrichten"
date: 2026-04-28
weight: 1
---

# Entwicklungsrechner einrichten

Diese Anleitung richtet eine vollständige lokale Entwicklungsumgebung für MeinCMS ein.

---

## Voraussetzungen

### Software installieren

**.NET 10 SDK**

```bash
# Ubuntu/Debian
sudo apt install -y dotnet-sdk-10.0

# Oder via Microsoft-Repository (empfohlen für neueste Version):
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update && sudo apt install -y dotnet-sdk-10.0
```

**PostgreSQL**

```bash
sudo apt install -y postgresql postgresql-contrib
```

**Entity Framework CLI-Tool**

```bash
dotnet tool install --global dotnet-ef
```

**Hugo** (für diese Dokumentation)

```bash
sudo apt install -y hugo
```

---

## Datenbank einrichten

```bash
sudo -u postgres psql
```

```sql
CREATE DATABASE meincmsdb;
CREATE USER meincmsuser WITH PASSWORD 'lokalesPasswort';
GRANT ALL PRIVILEGES ON DATABASE meincmsdb TO meincmsuser;
-- Ab PostgreSQL 15 zusätzlich:
\c meincmsdb
GRANT ALL ON SCHEMA public TO meincmsuser;
\q
```

---

## Projekt klonen und konfigurieren

```bash
git clone <repository-url>
cd wiki-ahrensburg.de
```

**Konfiguration anlegen** (aus der Vorlage):

```bash
cp mvc/_appsettings.json mvc/appsettings.json
```

`mvc/appsettings.json` anpassen:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=meincmsdb;Username=meincmsuser;Password=lokalesPasswort"
  },
  "Tenants": {
    "localhost": "main",
    "doc.localhost": "doc"
  },
  "TenantConfig": {
    "main": { "Title": "wiki-ahrensburg.de (lokal)", "Logo": "/logo.svg" },
    "doc":  { "Title": "Dokumentation (lokal)", "Logo": "/logo-doc.svg" }
  },
  "Kestrel": {
    "UnixSocket": ""
  }
}
```

> Den `UnixSocket`-Eintrag leer lassen – lokal hört Kestrel auf dem Standard-HTTP-Port.

---

## Datenbankmigrationen ausführen

```bash
dotnet ef database update --project mvc
```

---

## Anwendung starten

```bash
dotnet run --project mvc
```

Die Anwendung ist erreichbar unter:
- `http://localhost:5000` → Mandant `main`
- `http://doc.localhost:5000` → Mandant `doc` (hosts-Eintrag nötig, siehe unten)

**Optionaler hosts-Eintrag für zweiten Mandanten:**

```bash
# /etc/hosts
127.0.0.1   doc.localhost
```

---

## Ersten Administrator anlegen

```bash
dotnet run --project UserAdmin
```

---

## Entwicklungsworkflow

### Build prüfen

```bash
dotnet build
```

### Tests ausführen

```bash
dotnet test mvc.Tests
dotnet test Mardown.Tests

# Einzelne Testklasse
dotnet test mvc.Tests --filter "FullyQualifiedName~PageServiceTests"
```

### Neue Datenbankmigrierung erstellen

```bash
dotnet ef migrations add <MigrationName> --project mvc
dotnet ef database update --project mvc
```

### Nach Parser-Änderungen: HTML aller Artikel neu generieren

```bash
dotnet run --project backup -- repair
```

---

## Backup – Export und Import

Das Backup-Tool speichert Artikelinhalt ohne gerendertes HTML (ca. 70 % Größenersparnis). Beim Import wird das HTML automatisch neu erzeugt. Unterstützte Formate: **YAML** und **XML** – die Erkennung erfolgt automatisch anhand der Dateiendung.

### Export

**Alle Mandanten sichern (empfohlen):**

```bash
# YAML-Format
dotnet run --project backup -- export mein_backup.yaml --full

# XML-Format
dotnet run --project backup -- export mein_backup.xml --full
```

**Nur aktuellen Mandanten sichern** (ohne `--full`):

```bash
dotnet run --project backup -- export mein_backup.yaml
dotnet run --project backup -- export mein_backup.xml
```

**Automatischer Dateiname** (Datum und Uhrzeit im Namen):

```bash
# Erzeugt z. B.: backup_full_20260428_1430.yaml
dotnet run --project backup -- export --full
```

### Import

```bash
# Aus YAML
dotnet run --project backup -- import mein_backup.yaml

# Aus XML
dotnet run --project backup -- import mein_backup.xml
```

> Beim Import wird das gespeicherte HTML für jeden Artikel neu generiert. Das kann bei großen Datenbeständen einige Minuten dauern.

### HTML nach Parser-Änderungen neu generieren

Nach jeder Änderung am Markdown- oder WikiText-Parser muss das gespeicherte HTML aller Artikel neu erzeugt werden:

```bash
dotnet run --project backup -- repair
```

### Backup-Workflow: Daten auf neuen Rechner übertragen

```bash
# 1. Auf dem Quellrechner exportieren
dotnet run --project backup -- export umzug.xml --full

# 2. Datei auf Zielrechner kopieren (z. B. per scp oder USB)

# 3. Auf dem Zielrechner importieren
dotnet run --project backup -- import umzug.xml
```

---

## IDE-Empfehlungen

| IDE | Hinweis |
|---|---|
| **JetBrains Rider** | Empfohlen; EF-Migrationen über UI möglich |
| **Visual Studio Code** | Mit `C# Dev Kit`-Extension |

---

## Häufige Probleme

**`dotnet-ef` nicht gefunden nach Installation:**

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
# Dauerhaft in ~/.bashrc oder ~/.zshrc eintragen
```

**PostgreSQL-Verbindungsfehler `peer authentication`:**

```bash
# /etc/postgresql/*/main/pg_hba.conf anpassen:
# Zeile für "local" von "peer" auf "scram-sha-256" ändern
sudo systemctl restart postgresql
```

**Port 5000 bereits belegt:**

```bash
# Anderen Port verwenden:
ASPNETCORE_URLS="http://localhost:5001" dotnet run --project mvc
```
