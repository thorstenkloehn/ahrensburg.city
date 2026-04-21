/*
 * ahrensburg.city (MeinCMS) - A lightweight CMS with Wiki functionality and multi-tenancy.
 * Copyright (C) 2026 Thorsten
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
