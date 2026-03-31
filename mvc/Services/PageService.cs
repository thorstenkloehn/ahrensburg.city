using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Ganss.Xss;
using Mardown.Parser;
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
    private readonly MarkdownParser _markdownParser;
    private readonly HtmlSanitizer _sanitizer;
    private readonly IDeserializer _yamlDeserializer;
    private readonly ILogger<PageService> _logger;
    private readonly Wikitext.Parser.IMediaWikiParser _wikiParser;
    
    public const int MaxCategories = 10;
    public const int MaxCategoryLength = 50;

    public PageService(ApplicationDbContext context, ILogger<PageService> logger, Wikitext.Parser.IMediaWikiParser wikiParser)
    {
        _context = context;
        _logger = logger;
        _wikiParser = wikiParser;
        _markdownParser = new MarkdownParser();
        _sanitizer = new HtmlSanitizer();
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public async Task ErstelleOderAktualisiereWikiArtikelAsync(string slug, string wikiTextInhalt, List<string>? kategorien = null)
    {
        var extrahiertKategorien = _wikiParser.GetCategories(wikiTextInhalt);
        if (extrahiertKategorien.Any())
        {
            kategorien = (kategorien ?? new List<string>()).Concat(extrahiertKategorien).Distinct().ToList();
        }

        var htmlInhalt = _wikiParser.ToHtml(wikiTextInhalt);
        await SicherArtikelAsync(slug, htmlInhalt, null, wikiTextInhalt, kategorien);
    }

    public async Task ErstelleOderAktualisiereArtikelAsync(string slug, string markdownInhalt, List<string>? kategorien = null)
    {
        // YAML Frontmatter Extraktion
        var (extrahiertKategorien, _) = ExtrahiereMetadaten(markdownInhalt);
        if (extrahiertKategorien != null) kategorien = extrahiertKategorien;

        // MediaWiki-style Kategorien Extraktion [[kategorie:Name]]
        var mwKategorien = _markdownParser.GetCategories(markdownInhalt);
        if (mwKategorien.Any())
        {
            kategorien = (kategorien ?? new List<string>()).Concat(mwKategorien).Distinct().ToList();
        }

        var htmlInhalt = _markdownParser.ToHtml(markdownInhalt);
        await SicherArtikelAsync(slug, htmlInhalt, markdownInhalt, null, kategorien);
    }

    private async Task SicherArtikelAsync(string slug, string htmlInhalt, string? markdown, string? wikiText, List<string>? kategorien)
    {
        var sanitizedHtml = _sanitizer.Sanitize(htmlInhalt);
        var currentTenantId = _context.CurrentTenantId;

        // Kategorien Validierung
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

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var artikel = await _context.WikiArtikels
                .Include(a => a.Versionen)
                .FirstOrDefaultAsync(a => a.TenantId == currentTenantId && a.Slug == slug);

            if (artikel == null)
            {
                artikel = new WikiArtikel { Slug = slug, TenantId = currentTenantId };
                
                // Namensraum Erkennung
                if (slug.Contains(':'))
                {
                    var nsPart = slug.Split(':')[0];
                    var ns = await _context.WikiNamespaces
                        .FirstOrDefaultAsync(n => n.Name.ToLower() == nsPart.ToLower() || n.LocalizedName.ToLower() == nsPart.ToLower());
                    if (ns != null)
                    {
                        artikel.NamespaceId = ns.Id;
                    }
                }

                _context.WikiArtikels.Add(artikel);
            }

            var version = new WikiArtikelVersion
            {
                WikiArtikelId = artikel.Id,
                TenantId = currentTenantId,
                MarkdownInhalt = markdown,
                WikiTextInhalt = wikiText,
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

    public bool IstSlugGueltig(string slug)
    {
        if (string.IsNullOrEmpty(slug)) return false;
        if (!Regex.IsMatch(slug, @"^[\p{L}0-9/_\-\.%&:\ \(\)!,""`–]+$")) return false;
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

    public async Task<List<WikiArtikel>> GetArtikelNachKategorieAsync(string kategorie)
    {
        var currentTenantId = _context.CurrentTenantId;
        var moeglicheArtikelIds = await _context.WikiArtikelVersions
            .Where(v => v.TenantId == currentTenantId && v.Kategorie.Contains(kategorie))
            .Select(v => v.WikiArtikelId)
            .Distinct()
            .ToListAsync();

        if (!moeglicheArtikelIds.Any())
            return new List<WikiArtikel>();

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

        var neueVersion = new WikiArtikelVersion
        {
            WikiArtikelId = alteVersion.WikiArtikelId,
            TenantId = currentTenantId,
            MarkdownInhalt = alteVersion.MarkdownInhalt,
            WikiTextInhalt = alteVersion.WikiTextInhalt,
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
                            return (list.Select(o => o.ToString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList(), markdown);
                        else if (value is string s)
                            return (new List<string> { s }, markdown);
                    }
                }
            }
            catch { }
        }
        return (null, markdown);
    }

    public DiffPaneModel GenerateDiff(string oldContent, string newContent)
    {
        var differ = new InlineDiffBuilder(new DiffPlex.Differ());
        return differ.BuildDiffModel(oldContent, newContent);
    }
}
