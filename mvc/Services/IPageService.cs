namespace mvc.Services;

using mvc.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IPageService
{
    bool IstSlugGueltig(string slug);
    Task<WikiArtikel?> GetArtikelMitNeuesterVersionAsync(string slug);
    Task<WikiArtikel?> GetArtikelMitHistorieAsync(string slug);
    Task ErstelleOderAktualisiereArtikelAsync(string slug, string markdownInhalt, List<string>? kategorien = null);
    Task<List<WikiArtikel>> GetArtikelNachKategorieAsync(string kategorie);
    Task<bool> WiederherstellenAsync(long versionNummer);
    Task<WikiArtikelVersion?> GetVersionAsync(long versionNummer);
}
