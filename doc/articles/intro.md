# Einführung in MeinCMS

MeinCMS ist ein leichtgewichtiges Wiki-System auf Basis von **ASP.NET Core 10**.

## Architektur
- **MVC Webanwendung:** Die Benutzeroberfläche und das Routing.
- **Services:** Gemeinsam genutzte Logik für die Datenbank und Blogs.
- **UserAdmin:** Ein CLI-Tool zur Verwaltung von Benutzern.

## Automatische Dokumentation
Jeder Kommentar im Code, der mit `///` beginnt, wird automatisch in dieses Handbuch übernommen.
