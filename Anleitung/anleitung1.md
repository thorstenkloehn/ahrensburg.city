# Anleitung: Einheitliche Konfiguration (appsettings.json) für MeinCMS

Diese Anleitung erklärt, wie die Konfigurationsdateien (`appsettings.json`) in diesem Projekt verwaltet werden, um Probleme unter Linux (Ubuntu 24.04) zu vermeiden.

## 1. Das Problem (Warum der config-Ordner?)

Standardmäßig suchen .NET-Anwendungen die `appsettings.json` im gleichen Ordner wie das Programm. Da wir jedoch drei verschiedene Projekte haben (`mvc`, `UserAdmin`, `backup`), die alle dieselbe Datenbank-Konfiguration nutzen, gab es beim Veröffentlichen (`dotnet publish`) zwei Probleme:

1. **Dateikollisionen:** Auf Linux kann man im Veröffentlichungs-Ordner der Web-App keinen Unterordner namens `mvc` anlegen, wenn dort bereits eine Datei namens `mvc` (oder `mvc.dll`) existiert. Das führte zu Fehlermeldungen wie "Access Denied" oder "Ziel ist kein Ordner".
2. **Fehlende Dateien:** Konsolen-Tools wie `backup` kopieren standardmäßig keine Dateien aus anderen Projektordnern mit.

## 2. Die Lösung: Der `config/`-Unterordner

Alle drei Projekte wurden so umgestellt, dass sie die Konfiguration einheitlich in einem Unterordner namens `config/` suchen und dorthin kopieren.

### A. Einstellung in der Projektdatei (`.csproj`)
In jeder `.csproj`-Datei wurde dieser Block hinzugefügt. Er verlinkt die zentrale `appsettings.json` aus dem Web-Projekt und kopiert sie beim Build/Publish in den `config/`-Ordner:

```xml
<ItemGroup>
  <Content Include="..\mvc\appsettings.json">
    <Link>config\appsettings.json</Link>
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

### B. Einstellung im Programmcode (`Program.cs`)
Damit das Programm die Datei auch findet, sucht es nun an mehreren Orten nacheinander.

**Für die Web-App (mvc):**
```csharp
builder.Configuration.AddJsonFile("appsettings.json", optional: true);
builder.Configuration.AddJsonFile("config/appsettings.json", optional: true);
```

**Für die Konsolen-Tools (UserAdmin & backup):**
Der Code prüft nacheinander:
1. `./config/appsettings.json` (Wenn als fertiges Programm gestartet).
2. `./mvc/appsettings.json` (Wenn aus dem Hauptverzeichnis mit `dotnet run` gestartet).
3. `../mvc/appsettings.json` (Wenn aus dem Projektordner gestartet).

## 3. Richtig Veröffentlichen (Publish)

Wenn du das Projekt für den Server bereitmachen willst, nutze diese Befehle. Die Konfiguration wird automatisch in den Unterordner `config/` kopiert.

```bash
# Web-App veröffentlichen
dotnet publish mvc/mvc.csproj -o publish_web

# Administrator-Tool veröffentlichen
dotnet publish UserAdmin/UserAdmin.csproj -o publish_admin

# Backup-Tool veröffentlichen
dotnet publish backup/backup.csproj -o publish_backup
```

## 4. Wichtige Hinweise für Ubuntu 24.04

- **Git-Ignore:** Die Datei `mvc/appsettings.json` wird von Git ignoriert, um Passwörter zu schützen. Nach einem neuen Download des Projekts musst du sie erst aus der Vorlage erstellen:
  `cp mvc/_appsettings.json mvc/appsettings.json`
- **Schreibrechte:** Wenn du die Web-App über Unix-Sockets betreibst (siehe `Kestrel:UnixSocket` in der Konfiguration), stelle sicher, dass der Benutzer Schreibrechte für das Verzeichnis `/run/` hat oder ändere den Pfad in der `appsettings.json`.
