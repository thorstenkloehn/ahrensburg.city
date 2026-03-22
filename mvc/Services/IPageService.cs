namespace mvc.Services;

using mvc.Models;
using System.Threading.Tasks;

public interface IPageService
{
    bool IstSlugGueltig(string slug);
    Task<WikiArtikel?> GetArtikelMitNeuesterVersionAsync(string slug);
    Task<WikiArtikel?> GetArtikelMitHistorieAsync(string slug);
    Task ErstelleOderAktualisiereArtikelAsync(string slug, string markdownInhalt);
}
