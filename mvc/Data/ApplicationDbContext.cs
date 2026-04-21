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

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using mvc.Models;
using mvc.Services;

namespace mvc.Data;

public class ApplicationDbContext : IdentityDbContext
{
    private readonly ITenantService? _tenantService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService? tenantService = null) 
        : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<WikiArtikel> WikiArtikels { get; set; }
    public DbSet<WikiArtikelVersion> WikiArtikelVersions { get; set; }
    public DbSet<WikiNamespace> WikiNamespaces { get; set; }
    public DbSet<WikiCategory> WikiCategories { get; set; }

    /// <summary>
    /// Gibt die aktuelle TenantId dynamisch für den QueryFilter zurück.
    /// </summary>
    public string CurrentTenantId => _tenantService?.GetCurrentTenantId() ?? "main";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Globaler Abfragefilter für Mandantentrennung
        builder.Entity<WikiArtikel>()
            .HasQueryFilter(a => a.TenantId == CurrentTenantId);

        builder.Entity<WikiArtikelVersion>()
            .HasQueryFilter(v => v.TenantId == CurrentTenantId);

        builder.Entity<WikiCategory>()
            .HasQueryFilter(c => c.TenantId == CurrentTenantId);

        // Performance Indizes
        builder.Entity<WikiArtikel>()
            .HasIndex(a => new { a.TenantId, a.Slug })
            .IsUnique();

        builder.Entity<WikiArtikelVersion>()
            .HasIndex(v => new { v.TenantId, v.WikiArtikelId, v.Zeitpunkt });

        builder.Entity<WikiCategory>()
            .HasIndex(c => c.TenantId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentTenantId = _tenantService?.GetCurrentTenantId() ?? "main";

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is WikiArtikel artikel)
                {
                    if (string.IsNullOrEmpty(artikel.TenantId))
                    {
                        artikel.TenantId = currentTenantId;
                    }
                }
                else if (entry.Entity is WikiArtikelVersion version)
                {
                    if (string.IsNullOrEmpty(version.TenantId))
                    {
                        // Wir versuchen, die TenantId vom zugehörigen Artikel zu erben.
                        // Falls der Artikel selbst gerade erst angelegt wird und noch keine Id hat, 
                        // nutzen wir den aktuellen Tenant des Kontexts als Fallback.
                        var parentTenant = version.WikiArtikel?.TenantId;
                        
                        if (!string.IsNullOrEmpty(parentTenant))
                        {
                            version.TenantId = parentTenant;
                        }
                        else
                        {
                            version.TenantId = currentTenantId;
                        }
                    }
                }
                else if (entry.Entity is WikiCategory category)
                {
                    if (string.IsNullOrEmpty(category.TenantId))
                    {
                        category.TenantId = currentTenantId;
                    }
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
