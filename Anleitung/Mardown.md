# Anleitung: Markdown in MeinCMS

Diese Anleitung beschreibt, wie Inhalte im Wiki verfasst und strukturiert werden sollten.

## 1. Metadaten (YAML Frontmatter)
Um Kategorien für einen Artikel festzulegen, sollte am Anfang der Datei ein YAML-Block stehen. Dieser wird durch drei Bindestriche (`---`) eingeleitet und abgeschlossen.

**Beispiel:**
```yaml
---
Kategorie: [Technik, Programmierung, Hilfe]
---
```
*Hinweis: Wenn dieser Block vorhanden ist, überschreibt er die im Formular gewählten Kategorien.*

## 2. Textformatierung

### Überschriften
Verwenden Sie das Raute-Symbol (`#`) für Überschriften.
* `# Überschrift 1` (Titel des Artikels)
* `## Überschrift 2` (Sektions-Titel)
* `### Überschrift 3` (Untersektion)

### Listen
*   **Ungeordnete Liste:** Verwenden Sie `*`, `-` oder `+`.
*   **Geordnete Liste:** Verwenden Sie Zahlen gefolgt von einem Punkt (z.B. `1.`).

### Links und Bilder
*   **Interner Link:** `[Link Text](Slug-der-Seite)` (z.B. `[Startseite](Hauptseite)`)
*   **Externer Link:** `[Google](https://www.google.com)`
*   **Bilder:** `![Alt-Text](URL-zum-Bild)` *(Hinweis: Aktuell nur externe URLs möglich, da Upload deaktiviert)*

### Code
*   **Inline Code:** Umschließen Sie Text mit Backticks: `` `Code` ``.
*   **Code-Blöcke:** Verwenden Sie drei Backticks (```) vor und nach dem Block.

## 3. Sicherheitsregeln
*   **HTML:** Manuelles HTML ist erlaubt, wird aber durch einen **HtmlSanitizer** bereinigt. Gefährliche Tags (wie `<script>`) werden automatisch entfernt.
*   **Datei-Uploads:** Das Hochladen von Dateien ist aus Sicherheitsgründen untersagt.

## 4. Best Practices für Slugs
*   Verwenden Sie nur Buchstaben, Zahlen, Bindestriche (`-`) und Unterstriche (`_`).
*   Verwenden Sie Schrägstriche (`/`) für hierarchische Strukturen (z.B. `Anleitungen/Markdown`).
*   Vermeiden Sie Sonderzeichen und Leerzeichen.
