using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using mvc.Data;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Einheitliche Konfiguration aus Hauptordner UND config-Ordner laden
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("config/appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"config/appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Configure Kestrel for Unix Domain Sockets if requested
builder.WebHost.ConfigureKestrel(options =>
{
    var unixSocketPath = builder.Configuration["Kestrel:UnixSocket"];
    if (!string.IsNullOrEmpty(unixSocketPath))
    {
        if (File.Exists(unixSocketPath))
        {
            File.Delete(unixSocketPath);
        }
        options.ListenUnixSocket(unixSocketPath);
    }
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

if (builder.Environment.EnvironmentName != "Testing")
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});
builder.Services.AddResponseCaching();
builder.Services.AddOutputCache(options =>
{
    // Default policy: vary by host (Tenant) and slug
    options.AddBasePolicy(builder => builder.With(c => true).SetVaryByHost(true));
});
builder.Services.AddScoped<mvc.Services.ITenantService, mvc.Services.TenantService>();

builder.Services.AddDefaultIdentity<IdentityUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = true;
    
    // Password Policy
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;

    // Account Lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
    .AddRoles<IdentityRole>()





    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddScoped<mvc.Services.IPageService, mvc.Services.PageService>();
builder.Services.AddScoped<Wikitext.Parser.IMediaWikiTokenizer, Wikitext.Parser.MediaWikiTokenizer>();
builder.Services.AddScoped<Wikitext.Parser.IMediaWikiASTBuilder, Wikitext.Parser.MediaWikiASTBuilder>();
builder.Services.AddScoped<Wikitext.Parser.IMediaWikiASTSerializer, Wikitext.Parser.MediaWikiASTSerializer>();
builder.Services.AddScoped<Wikitext.Parser.IMediaWikiParser, Wikitext.Parser.MediaWikiParser>();

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

var app = builder.Build();

// Automatisierte Datenbank-Migrationen via Kommandozeile
if (args.Contains("--migrate"))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine("Datenbank-Migrationen werden angewendet...");
        dbContext.Database.Migrate();
        Console.WriteLine("Migrationen erfolgreich abgeschlossen.");
    }
    return;
}

// Configure Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' *.tile.openstreetmap.org data:; font-src 'self'; connect-src 'self' *.tile.openstreetmap.org;");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseRouting();

app.UseResponseCaching();
app.UseOutputCache();

app.UseAuthentication();
app.UseAuthorization();

// Admin No-Cache Middleware (placed after Authorization to have access to User and Roles)
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower() ?? "";
    bool isAdminArea = path.Contains("/edit/") || 
                       path.Contains("/create") || 
                       path.Contains("/neuformular") || 
                       path.Contains("/restore/") || 
                       path.Contains("/compare/") || 
                       path.Contains("/history/") || 
                       path.Contains("/version/") || 
                       path.Contains("/identity/account/");

    if (isAdminArea || (context.User.Identity?.IsAuthenticated == true && context.User.IsInRole("Admin")))
    {
        context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        context.Response.Headers.Pragma = "no-cache";
        context.Response.Headers.Expires = "0";
        
        // Disable Output Caching for this request
        var outputCacheFeature = context.Features.Get<Microsoft.AspNetCore.OutputCaching.IOutputCacheFeature>();
        if (outputCacheFeature != null)
        {
            outputCacheFeature.Context.AllowCacheLookup = false;
            outputCacheFeature.Context.AllowCacheStorage = false;
        }
    }
    await next();
});

app.MapStaticAssets();

app.MapControllers()
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();
app.Run();

public partial class Program { }
