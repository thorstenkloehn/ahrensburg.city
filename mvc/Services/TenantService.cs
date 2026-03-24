using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace mvc.Services;

/// <summary>
/// Implementierung des Mandanten-Dienstes basierend auf der Konfiguration (appsettings.json).
/// </summary>
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public TenantService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public string GetCurrentTenantId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "main";

        var host = context.Request.Host.Host.ToLower();

        // Alle konfigurierten Mandanten laden
        var tenantSection = _configuration.GetSection("Tenants");
        var entries = tenantSection.GetChildren().ToList();

        // 1. Alle Treffer finden (der Hostname enthält den konfigurierten Key)
        // Wir suchen nach dem LÄNGSTEN Key, um Subdomains Vorrang vor Hauptdomains zu geben.
        // Beispiel: "doc.ahrensburg.city" passt auf "ahrensburg.city" (15) und "doc.ahrensburg.city" (19).
        // Wir wählen den längsten Treffer (19).
        var bestMatch = entries
            .Where(e => host.Contains(e.Key.ToLower()))
            .OrderByDescending(e => e.Key.Length)
            .FirstOrDefault();

        if (bestMatch != null)
        {
            return bestMatch.Value ?? "main";
        }

        // Standard-Fallback
        return "main";
    }
}
