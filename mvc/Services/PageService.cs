using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Ganss.Xss;
using Markdig;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using mvc.Data;
using mvc.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace mvc.Services;

public class PageService : IPageService
{
    private readonly ApplicationDbContext _context;
    private readonly MarkdownPipeline _pipeline;
    private readonly HtmlSanitizer _sanitizer;
    private readonly IDeserializer _yamlDeserializer;
    private readonly ILogger<PageService> _logger;
    
    public const int MaxCategories = 10;
    public const int MaxCategoryLength = 50;

    public PageService(ApplicationDbContext context, ILogger<PageService> logger)
    {
        _context = context;
        _logger = logger;
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter()
            .Build();
        _sanitizer = new HtmlSanitizer();
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public bool IstSlugGueltig(string slug)
    {
        if (string.IsNullOrEmpty(slug)) return false;
        // Erlaubt Buchstaben (Unicode), Zahlen, Schrägstriche, Unterstriche, Bindestriche, Punkte und Prozentzeichen (für Encoded Slugs)
        if (!Regex.IsMatch(slug, @"^[\p{L}0-9/_\-\.%]+$")) return false;
        if (slug.Contains("..")) return false;
        if (slug.Contains("//")) return false;
        if (slug.StartsWith("/") || slug.EndsWith("/")) return false;
        return true;
    }

    public async Task<WikiArtikel?> GetArtikelMitNeuesterVersionAsync(string slug)
    {
        var currentTenantId = _context.CurrentTenantId;
        var artikel = await _context.WikiArtikels
            .FirstOrDefaultAsync(a => a.TenantId == currentTenantId && a.Slug == slug);
        
        if (artikel == null) return null;

        var neuesteVersion = await _context.WikiArtikelVersions
            .Where(v => v.TenantId == currentTenantId && v.WikiArtikelId == artikel.Id)
            .OrderByDescending(v => v.Zeitpunkt)
            .FirstOrDefaultAsync();

        artikel.Versionen = neuesteVersion != null 
            ? new List<WikiArtikelVersion> { neuesteVersion } 
            : new List<WikiArtikelVersion>();

        return artikel;
    }

    public async Task<WikiArtikel?> GetArtikelMitHistorieAsync(string slug)
    {
        var currentTenantId = _context.CurrentTenantId;
        return await _context.WikiArtikels
            .Include(a => a.Versionen)
            .FirstOrDefaultAsync(w => w.TenantId == currentTenantId && w.Slug == slug);
    }

    public async Task ErstelleOderAktualisiereArtikelAsync(string slug, string markdownInhalt, List<string>? kategorien = null)
    {
        // YAML Frontmatter Extraktion
        var (extrahiertKategorien, _) = ExtrahiereMetadaten(markdownInhalt);
        
        // YAML ist die primäre Quelle für Kategorien.
        if (extrahiertKategorien != null)
        {
            kategorien = extrahiertKategorien;
        }

        // Validierung
        if (kategorien != null)
        {
            if (kategorien.Count > MaxCategories)
                throw new ArgumentException($"Zu viele Kategorien (maximal {MaxCategories}).");

            foreach (var kat in kategorien)
            {
                if (kat.Length > MaxCategoryLength)
                    throw new ArgumentException($"Kategoriename '{kat}' ist zu lang (maximal {MaxCategoryLength} Zeichen).");
            }
        }

        var htmlInhalt = Markdown.ToHtml(markdownInhalt, _pipeline);
        var sanitizedHtml = _sanitizer.Sanitize(htmlInhalt);
        var currentTenantId = _context.CurrentTenantId;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var artikel = await _context.WikiArtikels
                .Include(a => a.Versionen)
                .FirstOrDefaultAsync(a => a.TenantId == currentTenantId && a.Slug == slug);

            if (artikel == null)
            {
                artikel = new WikiArtikel { Slug = slug, TenantId = currentTenantId };
                _context.WikiArtikels.Add(artikel);
            }

            var version = new WikiArtikelVersion
            {
                WikiArtikelId = artikel.Id,
                TenantId = currentTenantId,
                MarkdownInhalt = markdownInhalt,
                HtmlInhalt = sanitizedHtml,
                Zeitpunkt = DateTime.UtcNow,
                Kategorie = kategorien ?? new List<string>()
            };

            artikel.Versionen.Add(version);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<WikiArtikel>> GetArtikelNachKategorieAsync(string kategorie)
    {
        var currentTenantId = _context.CurrentTenantId;
        // 1. Wir suchen alle Artikel-IDs, die ÜBERHAUPT jemals dieser Kategorie zugeordnet waren.
        // Das reduziert die Datenmenge massiv und ist einfach zu übersetzen.
        var moeglicheArtikelIds = await _context.WikiArtikelVersions
            .Where(v => v.TenantId == currentTenantId && v.Kategorie.Contains(kategorie))
            .Select(v => v.WikiArtikelId)
            .Distinct()
            .ToListAsync();

        if (!moeglicheArtikelIds.Any())
            return new List<WikiArtikel>();

        // 2. Wir laden die Versions-Metadaten dieser Artikel, um die aktuellste Version zu bestimmen.
        // Wir filtern im Speicher (Client-side), da EF Core GroupBy.First() oft nicht übersetzen kann.
        var alleVersionenDerKandidaten = await _context.WikiArtikelVersions
            .Where(v => v.TenantId == currentTenantId && moeglicheArtikelIds.Contains(v.WikiArtikelId))
            .Select(v => new { v.WikiArtikelId, v.Zeitpunkt, v.Kategorie })
            .ToListAsync();

        var valideArtikelIds = alleVersionenDerKandidaten
            .GroupBy(v => v.WikiArtikelId)
            .Select(g => g.OrderByDescending(v => v.Zeitpunkt).First())
            .Where(v => v.Kategorie.Contains(kategorie))
            .Select(v => v.WikiArtikelId)
            .ToList();

        // 3. Die eigentlichen Artikel laden.
        return await _context.WikiArtikels
            .Where(a => a.TenantId == currentTenantId && valideArtikelIds.Contains(a.Id))
            .OrderBy(a => a.Slug)
            .ToListAsync();
    }

    public async Task<List<WikiArtikel>> GetAllArtikelAsync()
    {
        var currentTenantId = _context.CurrentTenantId;
        return await _context.WikiArtikels
            .Where(a => a.TenantId == currentTenantId)
            .OrderBy(a => a.Slug)
            .ToListAsync();
    }

    public async Task<bool> WiederherstellenAsync(long versionNummer)
    {
        var currentTenantId = _context.CurrentTenantId;
        var alteVersion = await _context.WikiArtikelVersions
            .Include(v => v.WikiArtikel)
            .FirstOrDefaultAsync(v => v.TenantId == currentTenantId && v.VersionNummer == versionNummer);

        if (alteVersion == null || alteVersion.WikiArtikel == null)
            return false;

        // Wir erstellen eine NEUE Version mit dem Inhalt der alten Version
        // So bleibt die Historie linear und nachvollziehbar
        var neueVersion = new WikiArtikelVersion
        {
            WikiArtikelId = alteVersion.WikiArtikelId,
            TenantId = currentTenantId,
            MarkdownInhalt = alteVersion.MarkdownInhalt,
            HtmlInhalt = alteVersion.HtmlInhalt,
            Kategorie = alteVersion.Kategorie ?? new List<string>(),
            Zeitpunkt = DateTime.UtcNow
        };

        _context.WikiArtikelVersions.Add(neueVersion);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<WikiArtikelVersion?> GetVersionAsync(long versionNummer)
    {
        var currentTenantId = _context.CurrentTenantId;
        return await _context.WikiArtikelVersions
            .Include(v => v.WikiArtikel)
            .FirstOrDefaultAsync(v => v.TenantId == currentTenantId && v.VersionNummer == versionNummer);
    }

    private (List<string>? kategorien, string inhalt) ExtrahiereMetadaten(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return (null, markdown);

        // Verbesserte Regex für YAML Frontmatter (beachtet verschiedene Zeilenumbrüche)
        var r = new Regex(@"^---\s*[\r\n]+(.*?)\s*[\r\n]+---\s*[\r\n]+", RegexOptions.Singleline);
        var match = r.Match(markdown);

        if (match.Success)
        {
            var yaml = match.Groups[1].Value;
            try
            {
                var metadata = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yaml);
                if (metadata != null)
                {
                    var key = metadata.Keys.FirstOrDefault(k => k.Equals("Kategorie", StringComparison.OrdinalIgnoreCase) 
                                                              || k.Equals("Categories", StringComparison.OrdinalIgnoreCase));
                    
                    if (key != null)
                    {
                        var value = metadata[key];
                        if (value is List<object> list)
                        {
                            return (list.Select(o => o.ToString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList(), markdown);
                        }
                        else if (value is string s)
                        {
                            return (new List<string> { s }, markdown);
                        }
                    }
                }
            }
            catch { }
        }

        return (null, markdown);
    }

    public DiffPaneModel GenerateDiff(string oldContent, string newContent)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var differ = new InlineDiffBuilder(new DiffPlex.Differ());
            return differ.BuildDiffModel(oldContent, newContent);
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation("Diff-Berechnung abgeschlossen in {ElapsedMilliseconds}ms (Alt: {OldLength} Zeichen, Neu: {NewLength} Zeichen)", 
                sw.ElapsedMilliseconds, oldContent.Length, newContent.Length);
        }
    }
}
