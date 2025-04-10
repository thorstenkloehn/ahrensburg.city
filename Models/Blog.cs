using System;
using System.ComponentModel.DataAnnotations;

namespace ahrensburg.city.Models;

public class Blog
{

public long Id { get; set; }
  [Required] // Stellt sicher, dass die Eigenschaft nicht null ist
  [MaxLength(255)] // Begrenzung auf maximal 255 Zeichen
    public string? Titel { get; set; }
   [Required] // Stellt sicher, dass die Eigenschaft nicht null ist
   public string? Inhaltmardown {get; set;}
   public string? Inhalthtml {get; set;}
  
     public DateTime Erstellungsdatum { get; set; } = DateTime.UtcNow;

    public DateTime? LetztesAenderungsdatum { get; set; }

}
