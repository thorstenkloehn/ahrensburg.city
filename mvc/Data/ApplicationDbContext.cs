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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Globaler Abfragefilter für Mandantentrennung
        var currentTenantId = _tenantService?.GetCurrentTenantId() ?? "main";

        builder.Entity<WikiArtikel>()
            .HasQueryFilter(a => a.TenantId == currentTenantId);

        builder.Entity<WikiArtikelVersion>()
            .HasQueryFilter(v => v.TenantId == currentTenantId);
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
