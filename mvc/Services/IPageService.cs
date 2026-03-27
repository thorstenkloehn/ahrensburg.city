using mvc.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiffPlex.DiffBuilder.Model;

namespace mvc.Services;

public interface IPageService
{
    bool IstSlugGueltig(string slug);
    Task<WikiArtikel?> GetArtikelMitNeuesterVersionAsync(string slug);
    Task<WikiArtikel?> GetArtikelMitHistorieAsync(string slug);
    Task ErstelleOderAktualisiereArtikelAsync(string slug, string markdownInhalt, List<string>? kategorien = null);
    Task ErstelleOderAktualisiereWikiArtikelAsync(string slug, string wikiTextInhalt, List<string>? kategorien = null);
    Task<List<WikiArtikel>> GetArtikelNachKategorieAsync(string kategorie);
    Task<List<WikiArtikel>> GetAllArtikelAsync();
    Task<bool> WiederherstellenAsync(long versionNummer);
    Task<WikiArtikelVersion?> GetVersionAsync(long versionNummer);
    DiffPaneModel GenerateDiff(string oldContent, string newContent);
}
