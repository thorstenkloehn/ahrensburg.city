using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using mvc.Data;
using mvc.Models;
using System.Xml.Serialization;

namespace backup;

class Program
{
    static async Task Main(string[] args)
    {
        // 1. Konfiguration laden
        string appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "config", "appsettings.json");
        if (!File.Exists(appSettingsPath))
        {
             appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "mvc", "appsettings.json");
        }
        if (!File.Exists(appSettingsPath))
        {
             appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "mvc", "appsettings.json");
        }
        if (!File.Exists(appSettingsPath))
        {
             appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "mvc", "appsettings.json");
        }
        
        if (!File.Exists(appSettingsPath))
        {
            Console.WriteLine($"Fehler: appsettings.json nicht gefunden.");
            Console.WriteLine($"Geprüfter Pfad: {appSettingsPath}");
            return;
        }

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
        
        // MediaWiki Parser registrieren
        services.AddScoped<mvc.Parser.IMediaWikiTokenizer, mvc.Parser.MediaWikiTokenizer>();
        services.AddScoped<mvc.Parser.IMediaWikiASTBuilder, mvc.Parser.MediaWikiASTBuilder>();
        services.AddScoped<mvc.Parser.IMediaWikiASTSerializer, mvc.Parser.MediaWikiASTSerializer>();
        services.AddScoped<mvc.Parser.IMediaWikiParser, mvc.Parser.MediaWikiParser>();
        
        var serviceProvider = services.BuildServiceProvider();

        // 3. Argumente verarbeiten
        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var wikiParser = scope.ServiceProvider.GetRequiredService<mvc.Parser.IMediaWikiParser>();

        var command = args[0].ToLower();

        if (command == "export")
        {
            bool full = args.Contains("--full");
            
            // Finde den ersten Parameter, der kein Befehl oder Flag ist
            string? fileName = args.FirstOrDefault(a => a != "export" && a != "--full");
            
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = full ? "meincms_full_backup.xml" : "meincms_backup.xml";
            }
            
            Console.WriteLine($"Exportiere {(full ? "ALLE" : "aktuelle")} Wiki-Inhalte in {fileName}...");
            await Export(context, fileName, full);
        }
        else if (command == "import")
        {
            string? fileName = args.FirstOrDefault(a => a != "import");
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                Console.WriteLine($"Fehler: Datei {fileName ?? "meincms_full_backup.xml"} nicht gefunden.");
                return;
            }
            Console.WriteLine($"Importiere Daten aus {fileName}...");
            await Import(context, wikiParser, fileName);
        }
        else
        {
            ShowHelp();
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("\n--- MeinCMS Backup Tool ---");
        Console.WriteLine("Verwendung:");
        Console.WriteLine("  dotnet run --project backup -- export [dateiname.xml|yaml] [--full]");
        Console.WriteLine("  dotnet run --project backup -- import [dateiname.xml|yaml]");
        Console.WriteLine("\nOptionen:");
        Console.WriteLine("  export         Erstellt ein Backup (Standard: meincms_backup.xml)");
        Console.WriteLine("  --full         Exportiert alle Artikel über alle Mandanten hinweg");
        Console.WriteLine("  import         Importiert Daten (Upsert-Logik)");
        Console.WriteLine("\nFormate:");
        Console.WriteLine("  .xml           Standard XML-Format");
        Console.WriteLine("  .yaml, .yml    Kompaktes YAML-Format (ohne HTML, inkl. Markdown & WikiText)");
    }

    static async Task Export(ApplicationDbContext context, string fileName, bool full)
    {
        var query = context.WikiArtikels.AsQueryable();
        if (full) query = query.IgnoreQueryFilters();

        var artikel = await query
            .Include(a => a.Versionen)
            .AsNoTracking()
            .ToListAsync();

        // Normalisierung: Leere TenantId zu "main"
        foreach (var a in artikel)
        {
            if (string.IsNullOrEmpty(a.TenantId)) a.TenantId = "main";
            if (a.Versionen != null)
            {
                foreach (var v in a.Versionen)
                {
                    if (string.IsNullOrEmpty(v.TenantId)) v.TenantId = "main";
                }
            }
        }

        if (fileName.EndsWith(".yaml") || fileName.EndsWith(".yml"))
        {
            var serializer = new YamlDotNet.Serialization.SerializerBuilder()
                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.PascalCaseNamingConvention.Instance)
                .Build();
            
            var directory = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var writer = new StreamWriter(fileName);
            serializer.Serialize(writer, artikel);
        }
        else
        {
            var serializer = new XmlSerializer(typeof(List<WikiArtikel>));
            var directory = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var writer = new StreamWriter(fileName))
            {
                serializer.Serialize(writer, artikel);
            }
        }

        Console.WriteLine($"ERFOLG: {artikel.Count} Artikel gesichert.");
    }

    static async Task Import(ApplicationDbContext context, mvc.Parser.IMediaWikiParser wikiParser, string fileName)
    {
        List<WikiArtikel>? importDaten = null;

        if (fileName.EndsWith(".yaml") || fileName.EndsWith(".yml"))
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.PascalCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            
            using var reader = new StreamReader(fileName);
            importDaten = deserializer.Deserialize<List<WikiArtikel>>(reader);
        }
        else
        {
            var serializer = new XmlSerializer(typeof(List<WikiArtikel>));
            using (var reader = new StreamReader(fileName))
            {
                importDaten = serializer.Deserialize(reader) as List<WikiArtikel>;
            }
        }

        if (importDaten == null) return;

        int neueArtikel = 0;
        int neueVersionen = 0;

        foreach (var bA in importDaten)
        {
            // Normalisierung beim Import
            var effectiveTenantId = string.IsNullOrEmpty(bA.TenantId) ? "main" : bA.TenantId;

            var dbA = await context.WikiArtikels.IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Slug == bA.Slug && a.TenantId == effectiveTenantId);
            
            if (dbA == null)
            {
                dbA = new WikiArtikel 
                { 
                    Slug = bA.Slug, 
                    TenantId = effectiveTenantId,
                    NamespaceId = bA.NamespaceId
                };
                context.WikiArtikels.Add(dbA);
                await context.SaveChangesAsync();
                neueArtikel++;
            }

            if (bA.Versionen != null)
            {
                foreach (var v in bA.Versionen.OrderBy(x => x.Zeitpunkt))
                {
                    // Auch hier Normalisierung der TenantId der Version
                    var effectiveVersionTenantId = string.IsNullOrEmpty(v.TenantId) ? effectiveTenantId : v.TenantId;

                    bool exists = await context.WikiArtikelVersions.IgnoreQueryFilters()
                        .AnyAsync(ev => ev.WikiArtikelId == dbA.Id && ev.Zeitpunkt == v.Zeitpunkt && ev.TenantId == effectiveVersionTenantId);

                    if (!exists)
                    {
                        string? html = v.HtmlInhalt;
                        if (string.IsNullOrEmpty(html))
                        {
                            if (!string.IsNullOrEmpty(v.MarkdownInhalt))
                            {
                                // Falls HTML fehlt (YAML/XML Export), regenerieren wir es aus Markdown
                                html = Markdig.Markdown.ToHtml(v.MarkdownInhalt);
                            }
                            else if (!string.IsNullOrEmpty(v.WikiTextInhalt))
                            {
                                // Falls HTML fehlt, regenerieren wir es aus WikiText
                                try 
                                {
                                    html = wikiParser.ToHtml(v.WikiTextInhalt);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Warnung: Fehler beim Parsen von WikiText für {bA.Slug} ({v.Zeitpunkt}): {ex.Message}");
                                    html = $"<div class='alert alert-danger'>Fehler beim Parsen von WikiText: {ex.Message}</div>";
                                }
                            }
                        }

                        context.WikiArtikelVersions.Add(new WikiArtikelVersion
                        {
                            WikiArtikelId = dbA.Id,
                            TenantId = effectiveVersionTenantId,
                            MarkdownInhalt = v.MarkdownInhalt,
                            WikiTextInhalt = v.WikiTextInhalt,
                            HtmlInhalt = html,
                            Kategorie = v.Kategorie,
                            Zeitpunkt = v.Zeitpunkt
                        });
                        neueVersionen++;
                    }
                }
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"IMPORT FERTIG: {neueArtikel} Artikel neu, {neueVersionen} Versionen hinzugefügt.");
    }
}
