function toggleSyntax() {
    var syntax = document.getElementById('syntax').value;
    if (syntax === 'markdown') {
        document.getElementById('markdown-container').classList.remove('d-none');
        document.getElementById('mediawiki-container').classList.add('d-none');
    } else {
        document.getElementById('markdown-container').classList.add('d-none');
        document.getElementById('mediawiki-container').classList.remove('d-none');
    }
}
