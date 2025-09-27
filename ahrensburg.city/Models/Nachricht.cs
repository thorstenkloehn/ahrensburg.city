using System;
using System.ComponentModel.DataAnnotations;
using Markdig;
namespace ahrensburg.city.Models;

public class Nachricht
{
    public long Id { get; set; }
    [Required(ErrorMessage = "Der Titel ist erforderlich.")]
    [StringLength(255, ErrorMessage = "Der Titel darf maximal 255 Zeichen lang sein.")]
    public string Titel { get; set; } = string.Empty;
    [Required(ErrorMessage = "Die Beschreibung ist erforderlich.")]
    [StringLength(500, ErrorMessage = "Die Beschreibung darf 500 Zeichen lang sein.")]
    public string Beschreibung { get; set; } = string.Empty;
     public string HtmlInhalt { get; private set; } = string.Empty;

    private string _mardown = string.Empty;

    [Required(ErrorMessage = "Der Inhalt darf nicht leer sein.")]
    public string Mardown
    {
        get => _mardown;
        set
        {
            _mardown = value ?? string.Empty;
            HtmlInhalt = string.IsNullOrWhiteSpace(_mardown)
                ? string.Empty
                : Markdown.ToHtml(_mardown);
        }
    }
}