using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using mvc.Data;
using mvc.Models;
using mvc.Services;
using Xunit;

namespace mvc.Tests;

public class PageServiceTests
{
    private ApplicationDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        return new ApplicationDbContext(options);
    }

    [Theory]
    [InlineData("gueltiger-slug", true)]
    [InlineData("mein_artikel123", true)]
    [InlineData("SlugMit/Slash", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("mit spaces", false)]
    [InlineData("mit..doppelpunkt", false)]
    [InlineData("mit//doppelslash", false)]
    [InlineData("/startslash", false)]
    [InlineData("endslash/", false)]
    [InlineData("umlaut-ä", false)]
    public void IstSlugGueltig_ValidatesCorrectly(string slug, bool expectedResult)
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new PageService(context);

        // Act
        var result = service.IstSlugGueltig(slug);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task ErstelleOderAktualisiereArtikelAsync_ErstelltNeuenArtikel()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new PageService(context);
        var slug = "test-artikel";
        var content = "# Hallo Welt";

        // Act
        await service.ErstelleOderAktualisiereArtikelAsync(slug, content);

        // Assert
        var result = await service.GetArtikelMitNeuesterVersionAsync(slug);
        Assert.NotNull(result);
        Assert.Equal(slug, result.Slug);
        Assert.Single(result.Versionen);
        Assert.Equal(content, result.Versionen[0].MarkdownInhalt);
        Assert.Contains("<h1>Hallo Welt</h1>", result.Versionen[0].HtmlInhalt);
    }

    [Fact]
    public async Task ErstelleOderAktualisiereArtikelAsync_FuegtNeueVersionHinzu()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new PageService(context);
        var slug = "test-artikel-versionen";
        
        await service.ErstelleOderAktualisiereArtikelAsync(slug, "# V1");

        // Act
        await service.ErstelleOderAktualisiereArtikelAsync(slug, "# V2");

        // Assert
        var result = await service.GetArtikelMitHistorieAsync(slug);
        Assert.NotNull(result);
        Assert.Equal(2, result.Versionen.Count);
        
        var neueste = await service.GetArtikelMitNeuesterVersionAsync(slug);
        Assert.NotNull(neueste);
        Assert.Single(neueste.Versionen);
        Assert.Equal("# V2", neueste.Versionen[0].MarkdownInhalt);
    }

    [Fact]
    public async Task ErstelleOderAktualisiereArtikelAsync_SpeichertKategorien()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new PageService(context);
        var slug = "kat-test";
        var kategorien = new List<string> { "News", "Wiki" };

        // Act
        await service.ErstelleOderAktualisiereArtikelAsync(slug, "# Inhalt", kategorien);

        // Assert
        var result = await service.GetArtikelMitNeuesterVersionAsync(slug);
        Assert.NotNull(result);
        Assert.Equal(kategorien, result.Versionen[0].Kategorie);
    }

    [Fact]
    public async Task ErstelleOderAktualisiereArtikelAsync_WirftFehlerBeiZuVielenKategorien()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new PageService(context);
        var kategorien = new List<string>();
        for (int i = 0; i < 11; i++) kategorien.Add("Kat" + i);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.ErstelleOderAktualisiereArtikelAsync("test", "# Inhalt", kategorien));
    }

    [Fact]
    public async Task ErstelleOderAktualisiereArtikelAsync_WirftFehlerBeiZuLangenKategorien()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = new PageService(context);
        var kategorien = new List<string> { new string('a', 51) };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => 
            service.ErstelleOderAktualisiereArtikelAsync("test", "# Inhalt", kategorien));
        Assert.Contains("ist zu lang", ex.Message);
    }
}
