# Anleitung zum Starten von MeinCMS

Dieses Projekt ist so vorkonfiguriert, dass es je nach Umgebung (Entwicklung oder Produktion) unterschiedliche Startmethoden verwendet.

## 1. Entwicklung (Lokal am PC)

In der Entwicklungsumgebung nutzt die App standardmäßig **TCP-Ports**, damit Sie sie direkt im Browser aufrufen können.

*   **Befehl:**
    ```bash
    dotnet run --project mvc --launch-profile http
    ```
*   **Adresse:** [http://localhost:5000](http://localhost:5000)
*   **Mandanten-Test:**
    Um Mandanten lokal zu testen, rufen Sie [http://doc.localhost:5000](http://doc.localhost:5000) auf (erfordert meist keine `hosts`-Anpassung).

## 2. Produktion (Linux-Server)

In der Produktion nutzt die App einen **Unix Domain Socket** (`/run/meincms.sock`). Dies ist performanter und sicherer für den Betrieb hinter einem Reverse Proxy.

*   **Befehl:**
    ```bash
    # Sicherstellen, dass die App im Produktions-Modus startet
    dotnet run --project mvc --configuration Release
    ```
*   **Nginx-Konfiguration (Beispiel):**
    ```nginx
    server {
        listen 80;
        server_name ahrensburg.city;

        location / {
            proxy_pass http://unix:/run/meincms.sock;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection keep-alive;
            proxy_set_header Host $host;
            proxy_cache_bypass $http_upgrade;
        }
    }
    ```
*   **Hinweis zu Berechtigungen:**
    Stellen Sie sicher, dass der Benutzer, unter dem die App läuft, Schreibrechte im Verzeichnis `/run/` hat (oder erstellen Sie `/run/meincms/` und passen Sie den Pfad in der `appsettings.json` an).

## 3. Automatisierte Tests

Um die gesamte Logik (inkl. Multi-Tenancy und Diff-Performance) zu validieren:

*   **Befehl:**
    ```bash
    dotnet test mvc.Tests
    ```
    Die Tests nutzen eine eigene `Testing`-Umgebung mit einer In-Memory-Datenbank.

---

### Zusammenfassung der Umgebungen

| Umgebung | Methode | Adresse / Socket | Konfigurationsdatei |
| :--- | :--- | :--- | :--- |
| **Development** | TCP Port | `http://localhost:5000` | `appsettings.Development.json` |
| **Production** | Unix Socket | `unix:/run/meincms.sock` | `appsettings.json` |
| **Testing** | In-Memory | N/A | (In Code konfiguriert) |
