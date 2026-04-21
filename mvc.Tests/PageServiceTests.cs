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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using mvc.Data;
using mvc.Models;
using mvc.Services;
using Xunit;
using Xunit.Abstractions;

namespace mvc.Tests;

public class PageServiceTests
{
    private readonly ITestOutputHelper _output;

    public PageServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private PageService CreateService(ApplicationDbContext context)
    {
        var tokenizer = new Wikitext.Parser.MediaWikiTokenizer();
        var astBuilder = new Wikitext.Parser.MediaWikiASTBuilder();
        var serializer = new Wikitext.Parser.MediaWikiASTSerializer();
        var wikiParser = new Wikitext.Parser.MediaWikiParser(tokenizer, astBuilder, serializer);
        var cache = new MemoryCache(new MemoryCacheOptions());
        return new PageService(context, NullLogger<PageService>.Instance, wikiParser, cache);
    }
    private ApplicationDbContext GetInMemoryContext(ITenantService? tenantService = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        return new ApplicationDbContext(options, tenantService);
    }

    [Theory]
    [InlineData("gueltiger-slug", true)]
    [InlineData("mein_artikel123", true)]
    [InlineData("SlugMit/Slash", true)]
    [InlineData("Geschichte_&_Allgemeines", true)]
    [InlineData("mit spaces", true)]
    [InlineData("Ahrensburg.city:Datenschutz", true)]
    [InlineData("CMS/Eleventy (11ty)", true)]
    [InlineData("Rust/\"Hello World!\" Programm", true)]
    [InlineData("Blog:03.02.2026 – Inhalt verwalten", true)]
    [InlineData("umlaut-ä", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("mit..doppelpunkt", false)]
    [InlineData("mit//doppelslash", false)]
    [InlineData("/startslash", false)]
    [InlineData("endslash/", false)]
    public void IstSlugGueltig_ValidatesCorrectly(string slug, bool expectedResult)
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = CreateService(context);

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
        var service = CreateService(context);
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
        var service = CreateService(context);
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
        var service = CreateService(context);
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
        var service = CreateService(context);
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
        var service = CreateService(context);
        var kategorien = new List<string> { new string('a', 51) };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => 
            service.ErstelleOderAktualisiereArtikelAsync("test", "# Inhalt", kategorien));
        Assert.Contains("ist zu lang", ex.Message);
    }

    [Fact]
    public async Task MultiTenancy_IsolatesArticlesBetweenTenants()
    {
        // Arrange
        var mockTenant1 = new Mock<ITenantService>();
        mockTenant1.Setup(t => t.GetCurrentTenantId()).Returns("tenant1");
        
        var mockTenant2 = new Mock<ITenantService>();
        mockTenant2.Setup(t => t.GetCurrentTenantId()).Returns("tenant2");

        var sharedDbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: sharedDbName)
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        // Act & Assert
        using (var context1 = new ApplicationDbContext(options, mockTenant1.Object))
        {
            var service1 = CreateService(context1);
            await service1.ErstelleOderAktualisiereArtikelAsync("shared-slug", "Content Tenant 1");
        }

        using (var context2 = new ApplicationDbContext(options, mockTenant2.Object))
        {
            var service2 = CreateService(context2);
            await service2.ErstelleOderAktualisiereArtikelAsync("shared-slug", "Content Tenant 2");

            var result2 = await service2.GetArtikelMitNeuesterVersionAsync("shared-slug");
            Assert.NotNull(result2);
            Assert.Equal("Content Tenant 2", result2.Versionen[0].MarkdownInhalt);
            
            // Verifiziere, dass wir nur EINEN Artikel sehen (den von tenant2)
            var count = await context2.WikiArtikels.CountAsync();
            Assert.Equal(1, count);
        }

        using (var context1 = new ApplicationDbContext(options, mockTenant1.Object))
        {
            var service1 = CreateService(context1);
            var result1 = await service1.GetArtikelMitNeuesterVersionAsync("shared-slug");
            Assert.NotNull(result1);
            Assert.Equal("Content Tenant 1", result1.Versionen[0].MarkdownInhalt);

            // Verifiziere, dass wir nur EINEN Artikel sehen (den von tenant1)
            var count = await context1.WikiArtikels.CountAsync();
            Assert.Equal(1, count);
        }
    }

    [Fact]
    public async Task MultiTenancy_KategorienAbfrage_IsoliertMandanten()
    {
        // Arrange
        var mockTenant1 = new Mock<ITenantService>();
        mockTenant1.Setup(t => t.GetCurrentTenantId()).Returns("tenant1");
        
        var mockTenant2 = new Mock<ITenantService>();
        mockTenant2.Setup(t => t.GetCurrentTenantId()).Returns("tenant2");

        var sharedDbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: sharedDbName)
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        using (var context1 = new ApplicationDbContext(options, mockTenant1.Object))
        {
            var service1 = CreateService(context1);
            await service1.ErstelleOderAktualisiereArtikelAsync("artikel1", "Inhalt", new List<string> { "TestKat" });
        }

        using (var context2 = new ApplicationDbContext(options, mockTenant2.Object))
        {
            var service2 = CreateService(context2);
            await service2.ErstelleOderAktualisiereArtikelAsync("artikel2", "Inhalt", new List<string> { "TestKat" });

            // Act
            var result = await service2.GetArtikelNachKategorieAsync("TestKat");

            // Assert
            Assert.Single(result);
            Assert.Equal("artikel2", result[0].Slug);
        }
    }

    [Fact]
    public async Task SucheArtikelAsync_FindetInhalte_BasicLogic()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = CreateService(context);
        await service.ErstelleOderAktualisiereArtikelAsync("suche-1", "Dies ist ein Test über Ahrensburg.");
        await service.ErstelleOderAktualisiereArtikelAsync("suche-2", "Hier geht es um etwas völlig anderes.");

        // Act
        var result1 = await service.SucheArtikelAsync("Ahrensburg");
        var result2 = await service.SucheArtikelAsync("völlig");
        var resultEmpty = await service.SucheArtikelAsync("NichtVorhanden");

        // Assert
        Assert.Single(result1);
        Assert.Equal("suche-1", result1[0].Slug);
        Assert.Single(result2);
        Assert.Equal("suche-2", result2[0].Slug);
        Assert.Empty(resultEmpty);
    }

    [Fact]
    public void GenerateDiff_Benchmark_LargeArticles()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var service = CreateService(context);
        
        // Generiere ca. 100.000 Zeichen Inhalt
        var baseContent = string.Join("\n", Enumerable.Range(0, 2000).Select(i => $"Zeile {i}: Das ist ein langer Satz für den Performance-Test."));
        var newContent = baseContent.Replace("Zeile 500", "Zeile 500 GEÄNDERT")
                                    .Replace("Zeile 1000", "Zeile 1000 GEÄNDERT")
                                    .Replace("Zeile 1500", "Zeile 1500 GEÄNDERT");

        // Act
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var diff = service.GenerateDiff(baseContent, newContent);
        sw.Stop();

        // Assert
        _output.WriteLine($"Diff-Dauer für {baseContent.Length} Zeichen: {sw.ElapsedMilliseconds}ms");
        Assert.NotNull(diff);
        // Wir setzen ein loses Limit von 2 Sekunden für 100k Zeichen (lokal meist < 100ms)
        Assert.True(sw.ElapsedMilliseconds < 2000, $"Diff dauerte zu lange: {sw.ElapsedMilliseconds}ms");
    }
}
