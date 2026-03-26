using System;
using System.ComponentModel.DataAnnotations;

namespace mvc.Models;

public class WikiArtikelVersion
{

    [Key]
    public long VersionNummer { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string? MarkdownInhalt { get; set; }

    [System.Xml.Serialization.XmlIgnore]
    [YamlDotNet.Serialization.YamlIgnore]
    public string? HtmlInhalt { get; set; }
    public DateTime Zeitpunkt { get; set; }
    public List<string> Kategorie { get; set; } = [];

    // Fremdschlüssel zum zugehörigen WikiArtikel
    public long WikiArtikelId { get; set; }
    
    [System.Xml.Serialization.XmlIgnore]
    [YamlDotNet.Serialization.YamlIgnore]
    public WikiArtikel? WikiArtikel { get; set; }

}
