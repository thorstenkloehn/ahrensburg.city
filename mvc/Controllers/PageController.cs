using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc.Data;
using mvc.Models;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using mvc.Services;

namespace mvc.Controllers
{
    /// <summary>
    /// Controller zur Verwaltung und Anzeige von Wiki-Seiten.
    /// </summary>
    public class PageController : Controller
    {
        
        private readonly IPageService _pageService;

        /// <summary>
        /// Initialisiert eine neue Instanz des <see cref="PageController"/>.
        /// </summary>
        /// <param name="pageService">Der Page Service.</param>
        public PageController(IPageService pageService)
        {
            _pageService = pageService;
        }

        /// <summary>
        /// Liefert die View für einen ungültigen Slug zurück.
        /// </summary>
        /// <param name="slug">Der ungültige Slug.</param>
        private ActionResult InvalidSlugResult(string slug)
        {
            ViewData["Slug"] = slug;
            return View("InvalidSlug");
        }

        /// <summary>
        /// Zeigt das Formular zum Erstellen einer neuen Wiki-Seite an.
        /// </summary>
        /// <param name="slug">Der gewünschte URL-Slug der neuen Seite.</param>
        /// <returns>Das View-Ergebnis für das Formular.</returns>
        [HttpGet("Neuformular/{*slug}")]
        [Authorize(Roles = "Admin")]
        public ActionResult Neuformular(string slug)
        {
            if (!string.IsNullOrEmpty(slug))
            {
                slug = System.Net.WebUtility.UrlDecode(slug);
            }

            if (!string.IsNullOrEmpty(slug) && !_pageService.IstSlugGueltig(slug))
                return InvalidSlugResult(slug);

            ViewBag.UrlSlug = slug;
            return View();
        }

        /// <summary>
        /// Erstellt eine neue Version eines Wiki-Artikels. Falls der Artikel noch nicht existiert, wird er angelegt.
        /// </summary>
        /// <param name="slug">Der Slug des Artikels.</param>
        /// <param name="markdownInhalt">Der Inhalt im Markdown-Format.</param>
        /// <returns>Eine Weiterleitung zur Index-Ansicht des Artikels.</returns>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(string slug, string markdownInhalt, string? kategorienRaw)
        {
            slug = System.Net.WebUtility.UrlDecode(slug);

            if (!_pageService.IstSlugGueltig(slug))
                return InvalidSlugResult(slug);

            if (string.IsNullOrWhiteSpace(markdownInhalt))
                return BadRequest("Inhalt darf nicht leer sein.");

            if (markdownInhalt.Length > 100000)
                return BadRequest("Inhalt ist zu lang (maximal 100.000 Zeichen).");

            var kategorien = kategorienRaw?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            try
            {
                await _pageService.ErstelleOderAktualisiereArtikelAsync(slug, markdownInhalt, kategorien);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            var encodedSlug = string.Join('/', slug.Split('/').Select(s => System.Net.WebUtility.UrlEncode(s)));
            return LocalRedirect("~/" + encodedSlug);
        }

        /// <summary>
        /// Zeigt das Bearbeitungsformular für eine bestehende Wiki-Seite an.
        /// </summary>
        /// <param name="slug">Der Slug der Seite.</param>
        /// <returns>Die Bearbeitungsansicht.</returns>
        [HttpGet("Edit/{*slug}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(string slug)
        {
            slug = System.Net.WebUtility.UrlDecode(slug);

            if (!_pageService.IstSlugGueltig(slug))
                return InvalidSlugResult(slug);

            var artikel = await _pageService.GetArtikelMitNeuesterVersionAsync(slug);

            if (artikel == null)
                return NotFound();

            ViewBag.UrlSlug = slug;
            return View(artikel);
        }

        /// <summary>
        /// Verarbeitet die Bearbeitung einer Wiki-Seite.
        /// </summary>
        /// <param name="slug">Der Slug der Seite.</param>
        /// <param name="markdownInhalt">Der neue Inhalt.</param>
        /// <returns>Eine Weiterleitung zur Index-Ansicht.</returns>
        [HttpPost("Edit/{*slug}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string slug, string markdownInhalt, string? kategorienRaw)
        {
            slug = System.Net.WebUtility.UrlDecode(slug);
            return await Create(slug, markdownInhalt, kategorienRaw);
        }

        /// <summary>
        /// Zeigt alle Wiki-Artikel einer bestimmten Kategorie an.
        /// </summary>
        /// <param name="kategorie">Die gewünschte Kategorie.</param>
        /// <returns>Eine Übersicht der Artikel.</returns>
        [HttpGet("Kategorie/{kategorie}")]
        public async Task<ActionResult> Kategorie(string kategorie)
        {
            if (string.IsNullOrEmpty(kategorie))
                return RedirectToAction("Index", "Page");

            var artikel = await _pageService.GetArtikelNachKategorieAsync(kategorie);
            ViewData["Kategorie"] = kategorie;
            return View(artikel);
        }

        /// <summary>
        /// Zeigt die Fehler-Seite an.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("Error/{code?}")]
        public IActionResult Error(int? code)
        {
            if (code == 404)
            {
                return View("NotFound404");
            }
            return View("../Shared/Error", new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Setzt einen Artikel auf eine bestimmte Version zurück.
        /// </summary>
        /// <param name="versionNummer">Die Nummer der Version, die wiederhergestellt werden soll.</param>
        /// <param name="slug">Der Slug des Artikels (für den Redirect).</param>
        [HttpPost("Restore/{versionNummer}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(long versionNummer, string slug)
        {
            var success = await _pageService.WiederherstellenAsync(versionNummer);
            if (!success)
                return NotFound();

            var encodedSlug = string.Join('/', slug.Split('/').Select(s => System.Net.WebUtility.UrlEncode(s)));
            return LocalRedirect("~/" + encodedSlug);
        }

        /// <summary>
        /// Vergleicht zwei Versionen eines Artikels (Diff-Ansicht).
        /// </summary>
        [HttpGet("Compare/{*slug}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Compare(string slug, long? v1, long? v2)
        {
            if (v1 == null || v2 == null) return BadRequest("Zwei Versionen müssen zum Vergleich angegeben werden.");
            
            var artikel = await _pageService.GetArtikelMitHistorieAsync(slug);
            if (artikel == null) return NotFound();

            var version1 = artikel.Versionen.FirstOrDefault(v => v.VersionNummer == v1);
            var version2 = artikel.Versionen.FirstOrDefault(v => v.VersionNummer == v2);

            if (version1 == null || version2 == null) return NotFound("Eine oder beide Versionen wurden nicht gefunden.");

            // Wir sortieren so, dass v1 die ältere und v2 die neuere Version ist (chronologisch)
            if (version1.Zeitpunkt > version2.Zeitpunkt)
            {
                var temp = version1;
                version1 = version2;
                version2 = temp;
            }

            ViewData["v1"] = version1;
            ViewData["v2"] = version2;
            ViewData["Slug"] = slug;

            var diff = _pageService.GenerateDiff(version1.MarkdownInhalt ?? "", version2.MarkdownInhalt ?? "");

            return View(diff);
        }

        /// <summary>
        /// Zeigt eine spezifische historische Version eines Artikels an.
        /// </summary>
        [HttpGet("Version/{versionNummer}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Version(long versionNummer)
        {
            var version = await _pageService.GetVersionAsync(versionNummer);

            if (version == null || version.WikiArtikel == null)
                return NotFound();

            ViewData["IsOldVersion"] = true;
            ViewData["Slug"] = version.WikiArtikel.Slug;
            
            // Wir "faken" ein WikiArtikel Objekt für die View, 
            // das nur diese eine Version in der Liste hat.
            var artikel = version.WikiArtikel;
            artikel.Versionen = new List<WikiArtikelVersion> { version };

            return View("Index", artikel);
        }

        /// <summary>
        /// Zeigt eine Wiki-Seite an. Wenn kein Slug angegeben ist, wird die "Hauptseite" geladen.
        /// </summary>
        /// <param name="slug">Der Slug der anzuzeigenden Seite.</param>
        /// <returns>Die Seite oder eine Fehlermeldung.</returns>
        [HttpGet("{*slug}")]
        public async Task<ActionResult> Index(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                slug = "Hauptseite";
            }
            else
            {
                slug = System.Net.WebUtility.UrlDecode(slug);
            }

            if (!_pageService.IstSlugGueltig(slug))
                return InvalidSlugResult(slug);

            var artikel = await _pageService.GetArtikelMitNeuesterVersionAsync(slug);

            if (artikel == null)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Neuformular", new { slug = slug });
                }
                return NotFound();
            }

            ViewBag.UrlSlug = slug;
            return View(artikel);
        }

        /// <summary>
        /// Zeigt die Versionshistorie eines Wiki-Artikels an.
        /// </summary>
        /// <param name="slug">Der Slug der Seite.</param>
        /// <returns>Die Historienansicht.</returns>
        [HttpGet("History/{*slug}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> History(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return InvalidSlugResult(slug);
            
            slug = System.Net.WebUtility.UrlDecode(slug);

            if (!_pageService.IstSlugGueltig(slug))
                return InvalidSlugResult(slug);

            var page = await _pageService.GetArtikelMitHistorieAsync(slug);

            if (page == null)
                return NotFound();

            ViewBag.UrlSlug = slug;
            return View("History", page);
        }
    }
}
