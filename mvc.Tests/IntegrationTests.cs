using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using mvc.Data;
using mvc.Services;
using System.Net;
using Xunit;
using Microsoft.AspNetCore.TestHost;

namespace mvc.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Environment", "Testing");
            builder.ConfigureServices(services =>
            {
                // Füge In-Memory DB hinzu
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb")
                           .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                });
            });
        });
    }

    [Fact]
    public async Task Get_Endpoints_ReturnSuccessAndCorrectContentType()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Füge eine Seite hinzu, damit "/" (Hauptseite) gefunden wird
        using (var scope = _factory.Services.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IPageService>();
            await service.ErstelleOderAktualisiereArtikelAsync("Hauptseite", "# Willkommen");
        }

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType!.ToString());
    }

    [Fact]
    public async Task SecurityHeaders_ArePresent()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.True(response.Headers.Contains("Referrer-Policy"));
        
        var csp = response.Headers.GetValues("Content-Security-Policy").First();
        Assert.Contains("default-src 'self'", csp);
    }

    [Fact]
    public async Task TenantTitle_IsCorrectlyLoaded()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "TenantConfig:test-tenant:Title", "Test-Mandant-Titel" }
                });
            });
            builder.ConfigureTestServices(services =>
            {
                var mockTenant = new Mock<ITenantService>();
                mockTenant.Setup(t => t.GetCurrentTenantId()).Returns("test-tenant");
                services.RemoveAll<ITenantService>(); // Vorherige Registrierung entfernen
                services.AddScoped<ITenantService>(_ => mockTenant.Object);
            });
        });

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Test-Mandant-Titel", html);
    }
}
