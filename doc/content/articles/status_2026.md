---
title: Projektstatus April 2026
last_updated: 2026-04-27
sources: [GEMINI.md]
---

# Projektstatus (Stand 24.04.2026)

Das Projekt hat im April 2026 einen bedeutenden Reifegrad erreicht und ist bereit für den produktiven Einsatz.

## Wichtige Meilensteine

### 1. Bereinigung & Optimierung (Streamlining)
Alle nicht mehr benötigten Kartendienste (Leaflet, GeoJSON) wurden entfernt. Das System ist nun schlanker und fokussiert sich auf seine Kernkompetenz als Wiki-CMS.

### 2. Multi-Tenancy
Die Mandantenfähigkeit ist vollständig implementiert. 
- **Mandant `main`:** Ahrensburg-spezifische Inhalte.
- **Mandant `doc`:** Technische Dokumentation.
- Die Trennung erfolgt auf Datenbankebene über globale Abfragefilter.

### 3. Parser-Evolution
Es stehen zwei leistungsstarke Parser zur Verfügung:
- **MediaWiki Parser Pro:** Unterstützt echte Absätze, verschachtelte Listen und Inline-HTML.
- **Markdown Parser:** Eigenentwickelter Parser ohne externe Abhängigkeiten, unterstützt YAML-Frontmatter.

### 4. Sicherheit & Deployment
- Erfolgreiches Sicherheits-Audit.
- Implementierung von Account-Lockout und strikter CSP.
- Unterstützung für Unix Domain Sockets für effizientes Nginx-Proxying.

### 5. Suche & Wartung
- **Volltextsuche:** Integriert in die Wiki-UI.
- **Backup & Repair 2.1:** Unterstützung für YAML/XML und ein Repair-Modus zur HTML-Regeneration.

## Offene Punkte (Roadmap)
- Implementierung eines Redirect-Systems für Slugs.
- Integration einer webbasierten Admin-UI für die Benutzerverwaltung.
