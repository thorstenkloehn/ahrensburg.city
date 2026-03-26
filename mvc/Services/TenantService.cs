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

        // Host ohne Port extrahieren
        var host = context.Request.Host.Host.ToLower();

        // Alle konfigurierten Mandanten laden
        var tenantSection = _configuration.GetSection("Tenants");
        var entries = tenantSection.GetChildren().ToList();

        // 1. Präziser Treffer (Hostname muss exakt übereinstimmen)
        var exactMatch = entries.FirstOrDefault(e => host.Equals(e.Key.ToLower()));

        if (exactMatch != null)
        {
            return exactMatch.Value ?? "main";
        }

        // Fallback: Wenn wir auf localhost sind, prüfen wir ob es ein Alias ist
        if (host == "localhost") return "main";

        // Standard-Fallback
        return "main";
    }
}
