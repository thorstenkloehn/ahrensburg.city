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
        string appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "mvc", "appsettings.json");
        if (!File.Exists(appSettingsPath))
             appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "mvc", "appsettings.json");
        
        if (!File.Exists(appSettingsPath))
        {
            Console.WriteLine($"Fehler: appsettings.json nicht gefunden.");
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
        
        var serviceProvider = services.BuildServiceProvider();

        // 3. Argumente verarbeiten
        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var command = args[0].ToLower();

        if (command == "export")
        {
            bool full = args.Contains("--full");
            string fileName = full ? "meincms_full_backup.xml" : "meincms_backup.xml";
            
            Console.WriteLine($"Exportiere {(full ? "ALLE" : "aktuelle")} Wiki-Inhalte in {fileName}...");
            await Export(context, fileName, full);
        }
        else if (command == "import")
        {
            string fileName = args.Length > 1 ? args[1] : "meincms_full_backup.xml";
            if (!File.Exists(fileName))
            {
                Console.WriteLine($"Fehler: Datei {fileName} nicht gefunden.");
                return;
            }
            Console.WriteLine($"Importiere Daten aus {fileName}...");
            await Import(context, fileName);
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
        Console.WriteLine("  dotnet run --project backup -- export [--full]");
        Console.WriteLine("  dotnet run --project backup -- import [dateiname.xml]");
        Console.WriteLine("\nOptionen:");
        Console.WriteLine("  export         Erstellt ein Backup (Standard: meincms_backup.xml)");
        Console.WriteLine("  --full         Exportiert alle Artikel über alle Mandanten hinweg");
        Console.WriteLine("  import         Importiert Daten (Upsert-Logik)");
    }

    static async Task Export(ApplicationDbContext context, string fileName, bool full)
    {
        var query = context.WikiArtikels.AsQueryable();
        if (full) query = query.IgnoreQueryFilters();

        var artikel = await query
            .Include(a => a.Versionen)
            .AsNoTracking()
            .ToListAsync();

        var serializer = new XmlSerializer(typeof(List<WikiArtikel>));
        using (var writer = new StreamWriter(fileName))
        {
            serializer.Serialize(writer, artikel);
        }

        Console.WriteLine($"ERFOLG: {artikel.Count} Artikel gesichert.");
    }

    static async Task Import(ApplicationDbContext context, string fileName)
    {
        List<WikiArtikel>? importDaten;
        var serializer = new XmlSerializer(typeof(List<WikiArtikel>));
        
        using (var reader = new StreamReader(fileName))
        {
            importDaten = serializer.Deserialize(reader) as List<WikiArtikel>;
        }

        if (importDaten == null) return;

        int neueArtikel = 0;
        int neueVersionen = 0;

        foreach (var bA in importDaten)
        {
            var dbA = await context.WikiArtikels.IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Slug == bA.Slug && a.TenantId == bA.TenantId);
            
            if (dbA == null)
            {
                dbA = new WikiArtikel { Slug = bA.Slug, TenantId = bA.TenantId };
                context.WikiArtikels.Add(dbA);
                await context.SaveChangesAsync();
                neueArtikel++;
            }

            if (bA.Versionen != null)
            {
                foreach (var v in bA.Versionen.OrderBy(x => x.Zeitpunkt))
                {
                    bool exists = await context.WikiArtikelVersions.IgnoreQueryFilters()
                        .AnyAsync(ev => ev.WikiArtikelId == dbA.Id && ev.Zeitpunkt == v.Zeitpunkt);

                    if (!exists)
                    {
                        context.WikiArtikelVersions.Add(new WikiArtikelVersion
                        {
                            WikiArtikelId = dbA.Id,
                            TenantId = dbA.TenantId,
                            MarkdownInhalt = v.MarkdownInhalt,
                            HtmlInhalt = v.HtmlInhalt,
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
