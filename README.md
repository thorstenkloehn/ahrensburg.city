# ahrensburg.city - MeinCMS

Ein Mandantenfähiges (Multi-Tenancy) Wiki-CMS System entwickelt mit ASP.NET Core 10 und PostgreSQL.

## Features

- **Mandantenfähigkeit**: Unterstützung für mehrere Domains/Mandanten (z.B. `ahrensburg.city` für Stadtinhalte und `doc.ahrensburg.city` für technische Dokumentation).
- **Wiki-Funktionalität**: Markdown-basierte Artikel mit voller Versionierung und Diff-Ansicht.
- **Sicherheit**:
  - `HtmlSanitizer` gegen XSS.
  - Strikte Content-Security-Policy (CSP).
  - CSRF-Schutz (Antiforgery-Token) global aktiviert.
  - Gehärtete Identity-Password-Policies.
- **Backup & Migration**: XML/YAML-Export/Import Tools für mandantenübergreifende Datenpflege.
- **Performance**: Unterstützung für Unix Domain Sockets für Nginx-Integration.

## Schnellstart

1.  **Datenbank**: PostgreSQL konfigurieren.
2.  **Appsettings**: `mvc/appsettings.json` anpassen.
3.  **Migrationen**: `dotnet ef database update --project mvc`
4.  **Starten**: `dotnet run --project mvc`

## Lizenz

Privates Projekt - Alle Rechte vorbehalten.
