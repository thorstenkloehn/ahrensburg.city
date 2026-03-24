# Sicherheitsaudit & Go-Live Bewertung - MeinCMS (24.03.2026)

Dieser Bericht bewertet den aktuellen Sicherheitsstatus nach der Umstellung auf eine flexible Multi-Tenancy-Architektur (Mandantenfähigkeit).

## 1. Bewertung der neuen Architektur (Multi-Tenancy)

### 1.1 Daten-Isolation - *Status: Sehr Sicher*
- **Implementierung:** Durch den Einsatz von **Global Query Filters** in EF Core ist sichergestellt, dass keine Datenbankabfrage ohne Mandantenfilter ausgeführt wird.
- **Sicherheitsgewinn:** Das Risiko, dass Mandant A versehentlich Daten von Mandant B sieht (Information Disclosure), wurde auf technologischer Ebene nahezu eliminiert.

### 1.2 Konfigurations-Sicherheit - *Status: Hoch*
- **Implementierung:** Die Zuordnung von Domains zu Mandanten erfolgt nun über die `appsettings.json`.
- **Analyse:** Es gibt keine fest codierten (hardcoded) Hostnamen mehr. Dies ermöglicht eine saubere Trennung von Code und Umgebungskonfiguration.

## 2. Analyse verbleibender Schwachstellen

### 2.1 Host-Header-Poisoning - *Status: Mittel (Aktion erforderlich)*
- **Problem:** Die Anwendung verlässt sich zur Identifikation des Mandanten auf den `Host`-Header des HTTP-Requests.
- **Risiko:** Ein Angreifer könnte versuchen, einen manipulierten Host-Header zu senden.
- **Empfehlung:** Beim Einsatz eines Reverse Proxy (wie Nginx) MUSS dieser so konfiguriert sein, dass nur bekannte Hostnamen an die Anwendung weitergereicht werden (`proxy_set_header Host $host`). Die Anwendung sollte zudem `AllowedHosts` in der `appsettings.json` strikt konfigurieren.

### 2.2 Fehlende Mandanten-Trennung bei Administratoren - *Status: Akzeptabel*
- **Problem:** Die Identity-Datenbank (Benutzerkonten) ist global. Ein Admin-Account gilt für das gesamte System (alle Mandanten).
- **Bewertung:** Da das System aktuell als Zentralinstanz für `ahrensburg.city` betrieben wird, ist dies ein gewünschtes Verhalten. Für den Betrieb von komplett fremden Mandanten wäre jedoch eine Isolation der Benutzerdaten notwendig.

### 2.3 Content Security Policy (CSP) - *Status: Bekanntes Restrisiko*
- **Problem:** `'unsafe-inline'` wird weiterhin benötigt.
- **Status:** Der `HtmlSanitizer` ist aktiv und schützt vor XSS. Das Restrisiko ist durch die restriktive Rollenverteilung (nur Admins dürfen editieren) minimiert.

## 3. Fehlerbehebung & Stabilität

- **Slug-Kollisionen:** Vollständig behoben. Identische Seitennamen (z.B. "Impressum") führen nicht mehr zu Datenbankfehlern.
- **Subdomain-Matching:** Durch die Einführung des "Best-Match"-Algorithmus im `TenantService` werden Subdomains (z.B. `doc.ahrensburg.city`) jetzt immer korrekt vor den Hauptdomains erkannt.

## 4. Abschließende Bewertung: Go-Live Empfehlung

**Status: GO-LIVE BEREIT (mit Nginx-Auflage)**

Die Website kann unter folgenden Bedingungen veröffentlicht werden:

1.  **Proxy-Konfiguration:** Der vorgelagerte Nginx muss zwingend den Host-Header absichern (siehe Bericht-Abschnitt 2.1).
2.  **HTTPS:** Der Betrieb ist nur mit TLS-Verschlüsselung (HTTPS) zulässig.
3.  **Wartung:** Die `appsettings.json` sollte auf dem Server schreibgeschützt sein, damit keine neuen Mandanten unbefugt hinzugefügt werden können.

**Fazit:** 
Durch die proaktive Implementierung der Global Query Filters ist das System im Vergleich zu Standard-Wiki-Systemen sehr gut gegen Datenlecks zwischen Mandanten geschützt. Die Architektur ist robust und bereit für den Produktivbetrieb beider Domains.

---
*Erstellt am: 24.03.2026, 19:30 Uhr*
*Status: Final / Freigabe erteilt*
