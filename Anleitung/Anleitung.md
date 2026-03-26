# MeinCMS - Umfassende Anleitung

Diese Anleitung beschreibt die Installation, den Betrieb und die Nutzung der Backup-Funktionen (Import/Export) für das Multi-Tenancy Wiki-CMS **ahrensburg.city (MeinCMS)**.

---

## 1. Systemvoraussetzungen

Bevor Sie beginnen, stellen Sie sicher, dass folgende Komponenten installiert sind:
*   **SDK:** [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
*   **Datenbank:** [PostgreSQL](https://www.postgresql.org/) (lokal oder als Server)
*   **Betriebssystem:** Linux (empfohlen für Produktion), macOS oder Windows.

---

## 2. Installation & Ersteinrichtung

### Schritt 1: Konfiguration vorbereiten
Die Anwendung benötigt eine `appsettings.json`, um die Datenbankverbindung zu kennen. Kopieren Sie die Vorlage im Verzeichnis `mvc/`:
```bash
cp mvc/_appsettings.json mvc/appsettings.json
```

### Schritt 2: Datenbankverbindung anpassen
Öffnen Sie `mvc/appsettings.json` und passen Sie den `DefaultConnection` String an Ihre PostgreSQL-Instanz an:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=meincms;Username=DEIN_USER;Password=DEIN_PASSWORT"
}
```

### Schritt 3: Datenbank-Migrationen ausführen
Erstellen Sie das Tabellenschema in Ihrer PostgreSQL-Datenbank:
```bash
dotnet ef database update --project mvc
```

---

## 3. Anwendung starten

### Entwicklung (Lokal)
Im Entwicklungsmodus nutzt die App TCP-Ports (Standard: 5000/5001).
```bash
dotnet run --project mvc --launch-profile http
```
*   **Adresse:** `http://localhost:5000`
*   **Multi-Tenancy Test:** Nutzen Sie `http://doc.localhost:5000`, um den technischen Mandanten zu sehen.

### Produktion (Linux-Server)
In der Produktion nutzt die App einen **Unix Domain Socket** (`/run/meincms.sock`) für bessere Performance hinter Nginx.
```bash
dotnet run --project mvc --configuration Release
```

---

## 4. Backup-Tool (Import & Export)

Das Projekt verfügt über ein spezialisiertes Tool (`backup/`), um Daten mandantenübergreifend zu sichern oder wiederherzustellen.

### Daten Exportieren
Sie können Daten in den Formaten **XML** oder **YAML** exportieren. YAML ist speichereffizienter, da der generierte HTML-Code weggelassen und beim Import regeneriert wird.

*   **Nur aktuellen Mandanten exportieren (XML):**
    ```bash
    dotnet run --project backup -- export sicherung.xml
    ```
*   **ALLE Mandanten und ALLE Artikel exportieren (Voll-Backup):**
    ```bash
    dotnet run --project backup -- export voll_sicherung.yaml --full
    ```

### Daten Importieren
Der Import nutzt eine **Upsert-Logik**: Er ergänzt fehlende Artikel und fügt neue Versionen hinzu, ohne bestehende Daten zu löschen.

*   **Import aus einer Datei:**
    ```bash
    dotnet run --project backup -- import voll_sicherung.yaml
    ```

**Wichtig:** Das Backup-Tool liest die Verbindungsdaten ebenfalls aus der `mvc/appsettings.json`.

---

## 5. Benutzerverwaltung

Da das System ein Wiki-CMS ist, benötigen Sie einen Administrator-Account für Bearbeitungen. Nutzen Sie dafür das `UserAdmin`-Projekt:
```bash
dotnet run --project UserAdmin
```
Folgen Sie den Anweisungen in der Konsole, um einen Benutzer anzulegen oder zum Administrator zu befördern.

---

## 6. Fehlerbehebung

*   **Appsettings nicht gefunden:** Stellen Sie sicher, dass Sie im Hauptverzeichnis des Projekts sind und `mvc/appsettings.json` existiert.
*   **Datenbankfehler:** Prüfen Sie, ob PostgreSQL läuft und der Benutzer die Rechte hat, Tabellen anzulegen.
*   **Socket-Fehler (Produktion):** Stellen Sie sicher, dass der Ordner `/run/` für den App-Benutzer beschreibbar ist oder ändern Sie den Pfad in der Konfigurationsdatei.
