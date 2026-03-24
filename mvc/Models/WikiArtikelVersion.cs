using System;
using System.ComponentModel.DataAnnotations;

namespace mvc.Models;

public class WikiArtikelVersion
{

    [Key]
    public long VersionNummer { get; set; }
    public string TenantId { get; set; } = "main";
    public string? MarkdownInhalt { get; set; }
    public string? HtmlInhalt { get; set; }
    public DateTime Zeitpunkt { get; set; }
    public List<string> Kategorie { get; set; } = [];

    // Fremdschlüssel zum zugehörigen WikiArtikel
    public long WikiArtikelId { get; set; }
    public WikiArtikel? WikiArtikel { get; set; }

}
