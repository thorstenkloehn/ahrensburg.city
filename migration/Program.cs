using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using mvc.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace migration;

/// <summary>
/// MediaWiki Migration Tool für ahrensburg.city
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("\n--- MediaWiki Migration Tool ---");
        
        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        var command = args[0].ToLower();
        if (command == "convert-xml")
        {
            // TODO: Konvertierung von MediaWiki XML Exporten zu MeinCMS YAML
            Console.WriteLine("Konvertierung von MediaWiki XML zu YAML wird vorbereitet...");
        }
        else if (command == "fetch-api")
        {
            // TODO: Direkter Abruf von Inhalten über die MediaWiki API (api.php)
            Console.WriteLine("Abruf von Inhalten über die MediaWiki API wird vorbereitet...");
        }
        else
        {
            ShowHelp();
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("\nVerwendung:");
        Console.WriteLine("  dotnet run --project migration -- convert-xml [mediawiki_dump.xml]");
        Console.WriteLine("  dotnet run --project migration -- fetch-api [api_url]");
        Console.WriteLine("\nZiel:");
        Console.WriteLine("  Erzeugt eine .yaml Datei, die mit dem 'backup' Tool importiert werden kann:");
        Console.WriteLine("  dotnet run --project backup -- import migration_result.yaml");
    }
}
