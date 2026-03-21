using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using mvc.Models;

namespace mvc.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<WikiArtikel> WikiArtikels { get; set; }
    public DbSet<WikiArtikelVersion> WikiArtikelVersions { get; set; }
 
}
