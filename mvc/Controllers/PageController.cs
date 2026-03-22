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
            // Slugs mit "/" werden von ASP.NET Core als echtes "/" im Pfad behandelt,
            // daher ist kein Uri.UnescapeDataString nötig.

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

            return LocalRedirect("~/" + slug);
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
            return await Create(slug, markdownInhalt, kategorienRaw);
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
            if (!_pageService.IstSlugGueltig(slug))
                return InvalidSlugResult(slug);

            var artikel = await _pageService.GetArtikelMitNeuesterVersionAsync(slug);

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
