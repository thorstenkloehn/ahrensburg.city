using ahrensburg.city.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ahrensburg.city.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public DbSet<Blog>Blogs{get;set;}
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
