using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace mvc.Models;

/// <summary>
/// Repräsentiert einen Wiki-Artikel im System.
/// </summary>
[Index(nameof(Slug), IsUnique = true)]
public class WikiArtikel
{
    /// <summary>
    /// Eindeutige ID des Artikels.
    /// </summary>
    [Key]
    public long Id { get; set; }
    
    /// <summary>
    /// Der eindeutige URL-Slug (Pfad) des Artikels.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Liste aller historischen und aktuellen Versionen dieses Artikels.
    /// </summary>
    public List<WikiArtikelVersion> Versionen { get; set; } = [];
}