---
title: Einführung in MeinCMS
last_updated: 2026-04-27
sources: [GEMINI.md, README.md]
---

# Einführung in MeinCMS

MeinCMS (wiki-ahrensburg.de) ist ein hochspezialisiertes, leichtgewichtiges Content-Management-System mit Wiki-Fokus. Es wurde entwickelt, um sowohl technische Dokumentation als auch regionale Inhalte (wie für die Stadt Ahrensburg) effizient und sicher zu verwalten.

## Kernphilosophie
- **Einfachheit:** Verzicht auf komplexe Abhängigkeiten; Nutzung nativer .NET 10.0 Features.
- **Mandantenfähigkeit:** Native Trennung von Inhalten (z. B. technisches Wiki vs. Stadt-Wiki) innerhalb einer einzigen Datenbank-Instanz.
- **Performance:** Optimiert für hohe Lasten durch Unix Domain Sockets und effiziente PostgreSQL-Abfragen.

## Hauptmerkmale
- **MediaWiki-Kompatibilität:** Ein eigener Compiler-Parser erlaubt die Nutzung von WikiText-Syntax.
- **Markdown-First:** Native Unterstützung für Markdown mit YAML-Frontmatter.
- **Sicherheit:** Integrierte HTML-Bereinigung, gehärtete Identitätsprüfung und CSRF-Schutz.
- **Backup & Recovery:** Robuste Tools für den Datenexport und die automatische Regeneration von Inhalten.

## Zielsetzung
Das System dient als zentraler Wissensknoten für **wiki-ahrensburg.de**, wobei es technische Flexibilität mit einfacher Bedienbarkeit kombiniert.
