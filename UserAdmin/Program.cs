using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using mvc.Data;

namespace UserAdmin;

class Program
{
    static async Task Main(string[] args)
    {
        // Konfiguration laden
        string appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "mvc", "appsettings.json");
        if (!File.Exists(appSettingsPath))
        {
             appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "mvc", "appsettings.json");
        }
        
        if (!File.Exists(appSettingsPath))
        {
            Console.WriteLine($"Fehler: appsettings.json nicht gefunden unter {appSettingsPath}.");
            return;
        }

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(appSettingsPath, optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Fehler: Connection string 'DefaultConnection' not found.");
            return;
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDataProtection();

        services.AddIdentity<IdentityUser, IdentityRole>(options => 
        {
            options.SignIn.RequireConfirmedAccount = true;
            // Erhöht auf 12 Zeichen, passend zur Web-App
            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Rolle sicherstellen
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Prüfen, ob bereits ein Admin existiert
            var admins = (await userManager.GetUsersInRoleAsync("Admin")).ToList();

            // WARNUNG statt automatischer Löschung
            if (admins.Count > 1)
            {
                Console.WriteLine($"\n--- WARNUNG: Mehrere Administratoren gefunden ({admins.Count}) ---");
                Console.WriteLine("Aus Sicherheitsgründen wird empfohlen, nur einen Notfall-Administrator zu führen.");
                Console.WriteLine("Aktuelle Administratoren:");
                foreach (var a in admins)
                {
                    Console.WriteLine($"- {a.UserName}");
                }
            }

            if (admins.Any())
            {
                var admin = admins.First();
                while (true)
                {
                    Console.WriteLine("\n--- Administrator & Backup Verwaltung ---");
                    Console.WriteLine($"Aktueller Fokus-Administrator: {admin.UserName}");
                    Console.WriteLine("1. Administrator Name (E-Mail) ändern");
                    Console.WriteLine("2. Administrator Passwort ändern");
                    Console.WriteLine("3. Wiki-Inhalt als XML exportieren (Backup)");
                    Console.WriteLine("4. Wiki-Inhalt aus XML importieren (Hinzufügen/Updaten)");
                    Console.WriteLine("5. Beenden");
                    Console.Write("Wählen Sie eine Option: ");
                    var choice = Console.ReadLine();

                    if (choice == "1")
                    {
                        Console.Write("Neuer Benutzername (E-Mail): ");
                        var newName = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(newName))
                        {
                            admin.UserName = newName;
                            admin.Email = newName;
                            var updateResult = await userManager.UpdateAsync(admin);
                            if (updateResult.Succeeded) 
                                Console.WriteLine("ERFOLG: Name erfolgreich geändert.");
                            else 
                                Console.WriteLine("FEHLER beim Ändern des Namens.");
                        }
                    }
                    else if (choice == "2")
                    {
                        Console.Write("Neues Passwort: ");
                        var newPassword = ReadPassword();
                        if (!string.IsNullOrWhiteSpace(newPassword))
                        {
                            var removeResult = await userManager.RemovePasswordAsync(admin);
                            var addResult = await userManager.AddPasswordAsync(admin, newPassword);
                            
                            if (addResult.Succeeded) 
                            {
                                Console.WriteLine("ERFOLG: Passwort erfolgreich geändert.");
                            }
                            else 
                            {
                                Console.WriteLine("FEHLER beim Ändern des Passworts:");
                                foreach (var error in addResult.Errors)
                                {
                                    Console.WriteLine($"- {error.Description}");
                                }
                            }
                        }
                    }
                    else if (choice == "3")
                    {
                        await ExportToXml(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
                    }
                    else if (choice == "4")
                    {
                        await ImportFromXml(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
                    }
                    else if (choice == "5")
                    {
                        break;
                    }
                }
                return;
            }

            // "Formular" zur Registrierung des einen Admins
            Console.WriteLine("--- Notfall-Administrator Registrierung ---");
            Console.WriteLine("Kein Administrator gefunden. Bitte legen Sie den Notfall-Account an.");
            
            Console.Write("Admin Benutzername (E-Mail) : ");
            var username = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(username)) return;

            Console.Write("Admin Passwort             : ");
            var password = ReadPassword();
            if (string.IsNullOrWhiteSpace(password)) return;

            var user = new IdentityUser 
            { 
                UserName = username, 
                Email = username, 
                EmailConfirmed = true 
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
                Console.WriteLine("\nERFOLG: Der Administrator wurde erstellt und die Rolle zugewiesen.");
                Console.WriteLine("Dieser Account ist nun der primäre Admin für Notfälle.");
            }
            else
            {
                Console.WriteLine("\nFEHLER beim Erstellen des Admins:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"- {error.Description}");
                }
            }
        }
    }

    static async Task ExportToXml(ApplicationDbContext context)
    {
        Console.Write("Dateiname für Backup (z.B. backup.xml): ");
        var fileName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(fileName)) return;

        var artikel = await context.WikiArtikels
            .IgnoreQueryFilters()
            .Include(a => a.Versionen)
            .AsNoTracking()
            .ToListAsync();

        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<mvc.Models.WikiArtikel>));
        using (var writer = new StreamWriter(fileName))
        {
            serializer.Serialize(writer, artikel);
        }

        Console.WriteLine($"\nERFOLG: {artikel.Count} Artikel wurden in {fileName} gesichert.");
    }

    static async Task ImportFromXml(ApplicationDbContext context)
    {
        Console.Write("Dateiname der Backup-Datei (XML): ");
        var fileName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
        {
            Console.WriteLine("Fehler: Datei nicht gefunden.");
            return;
        }

        List<mvc.Models.WikiArtikel>? importDaten;
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<mvc.Models.WikiArtikel>));
        
        using (var reader = new StreamReader(fileName))
        {
            importDaten = serializer.Deserialize(reader) as List<mvc.Models.WikiArtikel>;
        }

        if (importDaten == null)
        {
            Console.WriteLine("Fehler: XML konnte nicht geladen werden.");
            return;
        }

        Console.WriteLine($"Starte Import von {importDaten.Count} Artikeln...");
        int neueArtikel = 0;
        int neueVersionen = 0;

        foreach (var backupArtikel in importDaten)
        {
            // 1. Artikel finden oder erstellen
            var dbArtikel = await context.WikiArtikels
                .FirstOrDefaultAsync(a => a.Slug == backupArtikel.Slug && a.TenantId == backupArtikel.TenantId);
            
            if (dbArtikel == null)
            {
                dbArtikel = new mvc.Models.WikiArtikel 
                { 
                    Slug = backupArtikel.Slug, 
                    TenantId = backupArtikel.TenantId 
                };
                context.WikiArtikels.Add(dbArtikel);
                await context.SaveChangesAsync();
                neueArtikel++;
            }

            // 2. Versionen hinzufügen (Check über Zeitpunkt, um Duplikate zu vermeiden)
            if (backupArtikel.Versionen != null)
            {
                foreach (var v in backupArtikel.Versionen.OrderBy(x => x.Zeitpunkt))
                {
                    bool existiertBereits = await context.WikiArtikelVersions
                        .AnyAsync(ev => ev.WikiArtikelId == dbArtikel.Id && ev.Zeitpunkt == v.Zeitpunkt);

                    if (!existiertBereits)
                    {
                        var neueVersion = new mvc.Models.WikiArtikelVersion
                        {
                            WikiArtikelId = dbArtikel.Id,
                            TenantId = dbArtikel.TenantId,
                            MarkdownInhalt = v.MarkdownInhalt,
                            HtmlInhalt = v.HtmlInhalt,
                            Kategorie = v.Kategorie,
                            Zeitpunkt = v.Zeitpunkt
                        };
                        context.WikiArtikelVersions.Add(neueVersion);
                        neueVersionen++;
                    }
                }
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"\nIMPORT ABGESCHLOSSEN:");
        Console.WriteLine($"- Neue Artikel angelegt: {neueArtikel}");
        Console.WriteLine($"- Neue Versionen hinzugefügt: {neueVersionen}");
    }

    /// <summary>
    /// Liest ein Passwort von der Konsole ein, ohne es im Klartext anzuzeigen.
    /// </summary>
    static string ReadPassword()
    {
        string password = "";
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            else
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        }
        return password;
    }
}
