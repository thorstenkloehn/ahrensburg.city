/**
 * Konvertiert Markdown-Formatierung in MediaWiki-Formatierung.
 */
function convertMarkdownToMediaWiki(md) {
    if (!md) return '';
    let mw = md;
    // Überschriften: # Title -> == Title ==
    mw = mw.replace(/^#\s+(.*$)/gm, '== $1 ==');
    mw = mw.replace(/^##\s+(.*$)/gm, '=== $1 ===');
    mw = mw.replace(/^###\s+(.*$)/gm, '==== $1 ====');
    // Fett: **text** -> '''text'''
    mw = mw.replace(/\*\*(.*?)\*\*/g, "'''$1'''");
    // Kursiv: *text* -> ''text''
    mw = mw.replace(/\*(.*?)\*/g, "''$1''");
    // Links: [Text](URL) -> [URL Text]
    mw = mw.replace(/\[(.*?)\]\((https?:\/\/.*?)\)/g, '[$2 $1]');
    return mw;
}

/**
 * Konvertiert MediaWiki-Formatierung in Markdown-Formatierung.
 */
function convertMediaWikiToMarkdown(mw) {
    if (!mw) return '';
    let md = mw;
    // Überschriften: == Title == -> # Title
    md = md.replace(/^==\s+(.*?)\s+==$/gm, '# $1');
    md = md.replace(/^===(.*?)\s+===$/gm, '## $1');
    md = md.replace(/^====(.*?)\s+====$/gm, '### $1');
    // Fett: '''text''' -> **text**
    md = md.replace(/'''(.*?)'''/g, '**$1**');
    // Kursiv: ''text'' -> *text*
    md = md.replace(/''(.*?)''/g, '*$1*');
    // Links: [URL Text] -> [Text](URL)
    md = md.replace(/\[(https?:\/\/.*?)\s+(.*?)\]/g, '[$2]($1)');
    return md;
}

/**
 * Steuert die Sichtbarkeit der Editoren und die Inhalts-Konvertierung.
 */
function toggleSyntax() {
    console.log("toggleSyntax aufgerufen");
    var syntaxElement = document.getElementById('syntax');
    var markdownContainer = document.getElementById('markdown-container');
    var mediawikiContainer = document.getElementById('mediawiki-container');
    var markdownInhalt = document.getElementById('markdownInhalt');
    var wikiTextInhalt = document.getElementById('wikiTextInhalt');

    if (!syntaxElement || !markdownContainer || !mediawikiContainer) {
        console.error("Editor-Elemente nicht gefunden!");
        return;
    }

    var selectedSyntax = syntaxElement.value;
    console.log("Gewählte Syntax: " + selectedSyntax);

    if (selectedSyntax === 'markdown') {
        // Sichtbarkeit
        markdownContainer.classList.remove('d-none');
        markdownContainer.style.display = 'block';
        mediawikiContainer.classList.add('d-none');
        mediawikiContainer.style.display = 'none';
        
        // Konvertierung: Nur wenn Markdown leer ist, aber MediaWiki Inhalt hat
        if (markdownInhalt.value.trim() === '' && wikiTextInhalt.value.trim() !== '') {
            console.log("Konvertiere MediaWiki zu Markdown...");
            markdownInhalt.value = convertMediaWikiToMarkdown(wikiTextInhalt.value);
            wikiTextInhalt.value = '';
        }
    } else {
        // Sichtbarkeit
        markdownContainer.classList.add('d-none');
        markdownContainer.style.display = 'none';
        mediawikiContainer.classList.remove('d-none');
        mediawikiContainer.style.display = 'block';

        // Konvertierung: Nur wenn MediaWiki leer ist, aber Markdown Inhalt hat
        if (wikiTextInhalt.value.trim() === '' && markdownInhalt.value.trim() !== '') {
            console.log("Konvertiere Markdown zu MediaWiki...");
            wikiTextInhalt.value = convertMarkdownToMediaWiki(markdownInhalt.value);
            markdownInhalt.value = '';
        }
    }
}

// Initialisierung bei Seitenladung
document.addEventListener('DOMContentLoaded', function() {
    console.log("Editor-Skript geladen");
    
    // Initialen Status setzen
    toggleSyntax();
    
    // Event-Listener explizit binden (zusätzlich zum onchange im HTML)
    var syntaxElement = document.getElementById('syntax');
    if (syntaxElement) {
        syntaxElement.addEventListener('change', toggleSyntax);
    }
});
