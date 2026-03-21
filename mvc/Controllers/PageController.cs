using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc.Data;
using mvc.Models;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Ganss.Xss;

namespace mvc.Controllers
{
    /// <summary>
    /// Controller zur Verwaltung und Anzeige von Wiki-Seiten.
    /// </summary>
    public class PageController : Controller
    {
        
        private readonly ApplicationDbContext _context;
        private readonly MarkdownPipeline _pipeline;
        private readonly HtmlSanitizer _sanitizer;

        /// <summary>
        /// Prüft, ob ein Slug gültig ist (keine verbotenen Zeichen, keine '..', keine doppelten oder führenden/trailing Slashes).
        /// </summary>
        private bool IstSlugGueltig(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return false;
            if (!Regex.IsMatch(slug, @"^[a-zA-Z0-9/_-]+$")) return false;
            if (slug.Contains("..")) return false;
            if (slug.Contains("//")) return false;
            if (slug.StartsWith("/") || slug.EndsWith("/")) return false;
            return true;
        }

        /// <summary>
        /// Initialisiert eine neue Instanz des <see cref="PageController"/>.
        /// </summary>
        /// <param name="context">Der Datenbankkontext.</param>
        public PageController(ApplicationDbContext context)
        {
            _context = context;
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
            _sanitizer = new HtmlSanitizer();
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

            if (!string.IsNullOrEmpty(slug) && !IstSlugGueltig(slug))
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
        public async Task<IActionResult> Create(string slug, string markdownInhalt)
        {

            if (!IstSlugGueltig(slug))
                return InvalidSlugResult(slug);

            if (string.IsNullOrWhiteSpace(markdownInhalt))
                return BadRequest("Inhalt darf nicht leer sein.");

            if (markdownInhalt.Length > 100000)
                return BadRequest("Inhalt ist zu lang (maximal 100.000 Zeichen).");

            var htmlInhalt = Markdown.ToHtml(markdownInhalt, _pipeline);
            var sanitizedHtml = _sanitizer.Sanitize(htmlInhalt);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var artikel = await _context.WikiArtikels
                    .Include(a => a.Versionen)
                    .FirstOrDefaultAsync(a => a.Slug == slug);

                if (artikel == null)
                {
                    artikel = new WikiArtikel { Slug = slug };
                    _context.WikiArtikels.Add(artikel);
                }

                var version = new WikiArtikelVersion
                {
                    MarkdownInhalt = markdownInhalt,
                    HtmlInhalt = sanitizedHtml,
                    Zeitpunkt = DateTime.UtcNow
                };

                artikel.Versionen.Add(version);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return LocalRedirect("~/" + slug);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Zeigt das Bearbeitungsformular für eine bestehende Wiki-Seite an.
        /// </summary>
        /// <param name="slug">Der Slug der Seite.</param>
        /// <returns>Die Bearbeitungsansicht.</returns>
        [HttpGet("Edit/{*slug}")]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(string slug)
        {
            if (!IstSlugGueltig(slug))
                return InvalidSlugResult(slug);

            var artikel = _context.WikiArtikels
                .FirstOrDefault(a => a.Slug == slug);

            if (artikel == null)
                return NotFound();

            // Nur die neueste Version laden
            var neuesteVersion = _context.WikiArtikelVersions
                .Where(v => v.WikiArtikelId == artikel.Id)
                .OrderByDescending(v => v.Zeitpunkt)
                .FirstOrDefault();

            artikel.Versionen = neuesteVersion != null ? new List<WikiArtikelVersion> { neuesteVersion } : new List<WikiArtikelVersion>();

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
        public async Task<IActionResult> Edit(string slug, string markdownInhalt)
        {
            return await Create(slug, markdownInhalt);
        }

        /// <summary>
        /// Zeigt eine Wiki-Seite an. Wenn kein Slug angegeben ist, wird die "Hauptseite" geladen.
        /// </summary>
        /// <param name="slug">Der Slug der anzuzeigenden Seite.</param>
        /// <returns>Die Seite oder eine Fehlermeldung.</returns>
        [HttpGet("{*slug}")]
        public ActionResult Index(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                slug = "Hauptseite";
            }
            if (!IstSlugGueltig(slug))
                return InvalidSlugResult(slug);

            var artikel = _context.WikiArtikels
                .FirstOrDefault(a => a.Slug == slug);

            if (artikel != null)
            {
                // Nur die neueste Version laden
                var neuesteVersion = _context.WikiArtikelVersions
                    .Where(v => v.WikiArtikelId == artikel.Id)
                    .OrderByDescending(v => v.Zeitpunkt)
                    .FirstOrDefault();
                artikel.Versionen = neuesteVersion != null ? new List<WikiArtikelVersion> { neuesteVersion } : new List<WikiArtikelVersion>();
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
        public ActionResult History(string slug)
        {

            if (string.IsNullOrEmpty(slug))
                return InvalidSlugResult(slug);
            if (!IstSlugGueltig(slug))
                return InvalidSlugResult(slug);

            var page = _context.WikiArtikels
                .Include(a => a.Versionen)
                .FirstOrDefault(w => w.Slug == slug);

            if (page == null)
                return NotFound();

            ViewBag.UrlSlug = slug;
            return View("History", page);
        }
    }
}
