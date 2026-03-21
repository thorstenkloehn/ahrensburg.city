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
            options.Password.RequiredLength = 6;
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

            // Falls mehr als einer, alle außer dem ersten löschen (Anforderung: nur einer)
            if (admins.Count > 1)
            {
                Console.WriteLine($"\n--- WICHTIG: Mehrere Administratoren gefunden ({admins.Count}) ---");
                Console.WriteLine("Es ist nur ein Notfall-Administrator erlaubt. Zusätzliche werden gelöscht.");
                var principalAdmin = admins[0];
                for (int i = 1; i < admins.Count; i++)
                {
                    Console.WriteLine($"Lösche zusätzlichen Administrator: {admins[i].UserName}");
                    await userManager.DeleteAsync(admins[i]);
                }
                admins = new List<IdentityUser> { principalAdmin };
            }

            if (admins.Any())
            {
                var admin = admins.First();
                while (true)
                {
                    Console.WriteLine("\n--- Administrator Verwaltung ---");
                    Console.WriteLine($"Aktueller Administrator: {admin.UserName}");
                    Console.WriteLine("1. Administrator Name (E-Mail) ändern");
                    Console.WriteLine("2. Administrator Passwort ändern");
                    Console.WriteLine("3. Beenden");
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
                        var newPassword = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(newPassword))
                        {
                            // In einem Admin-Tool ist es oft sicherer, das Passwort direkt zu entfernen und neu zu setzen,
                            // da Token-basierte Resets manchmal an Konfigurationen (wie Data Protection) scheitern können.
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
            var password = Console.ReadLine();
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
                Console.WriteLine("Dieser Account ist nun der einzige Admin für Notfälle.");
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
}
