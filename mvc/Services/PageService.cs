namespace mvc.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ganss.Xss;
using Markdig;
using Microsoft.EntityFrameworkCore;
using mvc.Data;
using mvc.Models;

public class PageService : IPageService
{
    private readonly ApplicationDbContext _context;
    private readonly MarkdownPipeline _pipeline;
    private readonly HtmlSanitizer _sanitizer;
    
    public const int MaxCategories = 10;
    public const int MaxCategoryLength = 50;

    public PageService(ApplicationDbContext context)
    {
        _context = context;
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
        _sanitizer = new HtmlSanitizer();
    }

    public bool IstSlugGueltig(string slug)
    {
        if (string.IsNullOrEmpty(slug)) return false;
        if (!Regex.IsMatch(slug, @"^[a-zA-Z0-9/_-]+$")) return false;
        if (slug.Contains("..")) return false;
        if (slug.Contains("//")) return false;
        if (slug.StartsWith("/") || slug.EndsWith("/")) return false;
        return true;
    }

    public async Task<WikiArtikel?> GetArtikelMitNeuesterVersionAsync(string slug)
    {
        var artikel = await _context.WikiArtikels.FirstOrDefaultAsync(a => a.Slug == slug);
        if (artikel == null) return null;

        var neuesteVersion = await _context.WikiArtikelVersions
            .Where(v => v.WikiArtikelId == artikel.Id)
            .OrderByDescending(v => v.Zeitpunkt)
            .FirstOrDefaultAsync();

        artikel.Versionen = neuesteVersion != null 
            ? new List<WikiArtikelVersion> { neuesteVersion } 
            : new List<WikiArtikelVersion>();

        return artikel;
    }

    public async Task<WikiArtikel?> GetArtikelMitHistorieAsync(string slug)
    {
        return await _context.WikiArtikels
            .Include(a => a.Versionen)
            .FirstOrDefaultAsync(w => w.Slug == slug);
    }

    public async Task ErstelleOderAktualisiereArtikelAsync(string slug, string markdownInhalt, List<string>? kategorien = null)
    {
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
}
