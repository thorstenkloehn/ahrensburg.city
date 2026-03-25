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

    /// <summary>
    /// Gibt die aktuelle TenantId dynamisch für den QueryFilter zurück.
    /// </summary>
    public string CurrentTenantId => _tenantService?.GetCurrentTenantId() ?? "main";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Globaler Abfragefilter für Mandantentrennung - muss auf die Property zugreifen,
        // damit EF Core ihn bei jeder Abfrage dynamisch auswertet.
        builder.Entity<WikiArtikel>()
            .HasQueryFilter(a => a.TenantId == CurrentTenantId);

        builder.Entity<WikiArtikelVersion>()
            .HasQueryFilter(v => v.TenantId == CurrentTenantId);
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
                    artikel.TenantId = currentTenantId;
                }
                else if (entry.Entity is WikiArtikelVersion version)
                {
                    version.TenantId = currentTenantId;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
