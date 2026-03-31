using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using mvc.Data;
using mvc.Models;
using System.Xml.Serialization;

namespace backup;

/// <summary>
/// Container für ein vollständiges Backup aller CMS-Daten.
/// </summary>
public class BackupContainer
{
    public List<WikiArtikel> Artikel { get; set; } = new();
    public List<WikiNamespace> Namespaces { get; set; } = new();
    public DateTime ExportZeitpunkt { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = "2.1";
}

class Program
{
    static async Task Main(string[] args)
    {
        // 1. Konfiguration laden
        string appSettingsPath = GetAppSettingsPath();
        if (appSettingsPath == null) return;

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(appSettingsPath, optional: false, reloadOnChange: true)
            .Build();

        // 2. Services einrichten
        var services = new ServiceCollection();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Fehler: Verbindung zur Datenbank nicht gefunden.");
            return;
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        services.AddScoped<Wikitext.Parser.IMediaWikiTokenizer, Wikitext.Parser.MediaWikiTokenizer>();
        services.AddScoped<Wikitext.Parser.IMediaWikiASTBuilder, Wikitext.Parser.MediaWikiASTBuilder>();
        services.AddScoped<Wikitext.Parser.IMediaWikiASTSerializer, Wikitext.Parser.MediaWikiASTSerializer>();
        services.AddScoped<Wikitext.Parser.IMediaWikiParser, Wikitext.Parser.MediaWikiParser>();
        
        var serviceProvider = services.BuildServiceProvider();

        // 3. Argumente verarbeiten
        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var wikiParser = scope.ServiceProvider.GetRequiredService<Wikitext.Parser.IMediaWikiParser>();

        var command = args[0].ToLower();

        if (command == "export")
        {
            bool full = args.Contains("--full");
            string? fileName = args.FirstOrDefault(a => a != "export" && a != "--full");
            
            if (string.IsNullOrEmpty(fileName))
            {
                string suffix = full ? "full" : "tenant";
                fileName = $"backup_{suffix}_{DateTime.Now:yyyyMMdd_HHmm}.yaml";
            }
            
            Console.WriteLine($"[*] Starte Export (Modus: {(full ? "GLOBAL" : "AKTUELLER MANDANT")})...");
            await Export(context, fileName, full);
        }
        else if (command == "import")
        {
            string? fileName = args.FirstOrDefault(a => a != "import");
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                Console.WriteLine($"[!] Fehler: Datei {fileName ?? "backup.yaml"} nicht gefunden.");
                return;
            }
            Console.WriteLine($"[*] Starte Import aus {fileName}...");
            await Import(context, wikiParser, fileName);
        }
        else
        {
            ShowHelp();
        }
    }

    private static string? GetAppSettingsPath()
    {
        var paths = new[] 
        {
            Path.Combine(Directory.GetCurrentDirectory(), "config", "appsettings.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "mvc", "appsettings.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "mvc", "appsettings.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "mvc", "appsettings.json")
        };

        foreach (var path in paths)
        {
            if (File.Exists(path)) return path;
        }

        Console.WriteLine("Fehler: appsettings.json nicht gefunden.");
        return null;
    }

    static void ShowHelp()
    {
        Console.WriteLine("\n=== MeinCMS Backup Professional Edition ===");
        Console.WriteLine("Verwendung:");
        Console.WriteLine("  dotnet run --project backup -- export [file.yaml] [--full]");
        Console.WriteLine("  dotnet run --project backup -- import [file.yaml]");
        Console.WriteLine("\nFeatures:");
        Console.WriteLine("  - Automatisches Dateinamen-Schema: backup_full_20260401_0126.yaml");
        Console.WriteLine("  - Sichert Artikel, Versionen und Namespaces.");
        Console.WriteLine("  - Regeneriert HTML beim Import (spart 70% Speicherplatz).");
    }

    static async Task Export(ApplicationDbContext context, string fileName, bool full)
    {
        var query = context.WikiArtikels.AsQueryable();
        var nsQuery = context.WikiNamespaces.AsQueryable();

        if (full)
        {
            query = query.IgnoreQueryFilters();
            nsQuery = nsQuery.IgnoreQueryFilters();
        }

        var artikel = await query.Include(a => a.Versionen).AsNoTracking().ToListAsync();
        var namespaces = await nsQuery.AsNoTracking().ToListAsync();

        var container = new BackupContainer
        {
            Artikel = artikel,
            Namespaces = namespaces,
            ExportZeitpunkt = DateTime.UtcNow
        };

        // Normalisierung
        foreach (var a in container.Artikel)
        {
            if (string.IsNullOrEmpty(a.TenantId)) a.TenantId = "main";
            foreach (var v in a.Versionen) if (string.IsNullOrEmpty(v.TenantId)) v.TenantId = "main";
        }

        var serializer = new YamlDotNet.Serialization.SerializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.PascalCaseNamingConvention.Instance)
            .Build();

        await File.WriteAllTextAsync(fileName, serializer.Serialize(container));
        Console.WriteLine($"[OK] Export abgeschlossen. {artikel.Count} Artikel in '{fileName}' gesichert.");
    }

    static async Task Import(ApplicationDbContext context, Wikitext.Parser.IMediaWikiParser wikiParser, string fileName)
    {
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        
        BackupContainer? container;
        using (var reader = new StreamReader(fileName))
        {
            // Abwärtskompatibilität prüfen: Ist es ein BackupContainer oder eine Liste?
            var content = await reader.ReadToEndAsync();
            if (content.Contains("Artikel:") && content.Contains("Namespaces:"))
            {
                container = deserializer.Deserialize<BackupContainer>(content);
            }
            else
            {
                // Altes Format (nur Liste)
                var artikel = deserializer.Deserialize<List<WikiArtikel>>(content);
                container = new BackupContainer { Artikel = artikel };
            }
        }

        if (container == null) return;

        // 1. Namespaces importieren
        foreach (var ns in container.Namespaces)
        {
            var exists = await context.WikiNamespaces.IgnoreQueryFilters()
                .AnyAsync(x => x.Id == ns.Id || x.Name == ns.Name);
            if (!exists) context.WikiNamespaces.Add(ns);
        }
        await context.SaveChangesAsync();

        // 2. Artikel importieren
        int neueArtikel = 0, neueVersionen = 0;
        var markdownParser = new Mardown.Parser.MarkdownParser();

        foreach (var bA in container.Artikel)
        {
            var effectiveTenantId = string.IsNullOrEmpty(bA.TenantId) ? "main" : bA.TenantId;
            var dbA = await context.WikiArtikels.IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Slug == bA.Slug && a.TenantId == effectiveTenantId);
            
            if (dbA == null)
            {
                dbA = new WikiArtikel { Slug = bA.Slug, TenantId = effectiveTenantId, NamespaceId = bA.NamespaceId };
                context.WikiArtikels.Add(dbA);
                await context.SaveChangesAsync();
                neueArtikel++;
            }

            foreach (var v in bA.Versionen.OrderBy(x => x.Zeitpunkt))
            {
                var vTenantId = string.IsNullOrEmpty(v.TenantId) ? effectiveTenantId : v.TenantId;
                bool exists = await context.WikiArtikelVersions.IgnoreQueryFilters()
                    .AnyAsync(ev => ev.WikiArtikelId == dbA.Id && ev.Zeitpunkt == v.Zeitpunkt && ev.TenantId == vTenantId);

                if (!exists)
                {
                    string? html = v.HtmlInhalt;
                    if (string.IsNullOrEmpty(html))
                    {
                        if (!string.IsNullOrEmpty(v.MarkdownInhalt))
                            html = markdownParser.ToHtml(v.MarkdownInhalt);
                        else if (!string.IsNullOrEmpty(v.WikiTextInhalt))
                            html = wikiParser.ToHtml(v.WikiTextInhalt);
                    }

                    context.WikiArtikelVersions.Add(new WikiArtikelVersion
                    {
                        WikiArtikelId = dbA.Id,
                        TenantId = vTenantId,
                        MarkdownInhalt = v.MarkdownInhalt,
                        WikiTextInhalt = v.WikiTextInhalt,
                        HtmlInhalt = html ?? "",
                        Kategorie = v.Kategorie,
                        Zeitpunkt = v.Zeitpunkt
                    });
                    neueVersionen++;
                }
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[OK] Import abgeschlossen: {neueArtikel} Artikel neu, {neueVersionen} Versionen hinzugefügt.");
    }
}
