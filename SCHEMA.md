# Wissensmanagement-Schema (SCHEMA.md)

Dieses Dokument definiert die Struktur und die Arbeitsabläufe für das KI-gestützte Wissensmanagement in diesem Projekt.

## 1. Die drei Ebenen

### Ebene 1: Rohdaten (`/sources/`)
- **Status:** Unveränderlich (Read-Only für das LLM).
- **Inhalt:** Quelldokumente, Artikel, Datensätze, Bilder, Protokolle.
- **Regel:** Das LLM liest diese Daten, verändert sie aber niemals. Neue Rohdaten werden vom Benutzer hinzugefügt.

### Ebene 2: Das Wiki (`/doc/`)
- **Status:** LLM-verwaltet (Read-Write für das LLM).
- **Inhalt:** Synthesen, Zusammenfassungen, Objektseiten, Konzepte, Querverweise.
- **Regel:** Das LLM erstellt und aktualisiert diese Markdown-Dateien basierend auf den Rohdaten. Manuelle Änderungen durch den Benutzer sind möglich, sollten aber vom LLM im nächsten Zyklus berücksichtigt werden.

### Ebene 3: Das Schema (`SCHEMA.md`)
- **Status:** Gemeinsame Konfiguration.
- **Inhalt:** Strukturvorgaben, Namenskonventionen, Workflows.
- **Regel:** Definiert, wie der Wiki-Manager (LLM) agiert.

## 2. Workflows

### Integration neuer Quellen
1. Der Benutzer legt eine neue Datei in `/sources/` ab.
2. Das LLM analysiert die neue Quelle.
3. Das LLM aktualisiert bestehende Wiki-Seiten in `/doc/` oder erstellt neue Seiten.
4. Das LLM pflegt Querverweise (Links) zwischen den Seiten.

### Beantwortung von Fragen
- Fragen werden primär auf Basis des **Wikis** beantwortet.
- Bei Unklarheiten oder für tiefere Details zieht das LLM die **Rohdaten** heran.

## 3. Konventionen für das Wiki (`/doc/`)
- **Format:** Markdown (.md).
- **Struktur:** 
  - `index.md`: Hauptübersicht.
  - `articles/`: Detailartikel und Synthesen.
- **Metadaten:** Jede Seite sollte YAML-Frontmatter enthalten (Titel, Datum der letzten Aktualisierung, Quellenbezug).
- **Sprache:** Deutsch (sofern nicht anders gewünscht).
