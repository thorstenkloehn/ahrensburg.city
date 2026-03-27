using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace mvc.Models;

public class WikiNamespace
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. "Benutzer", "Kategorie", "Datei"
    public string LocalizedName { get; set; } = string.Empty;
    public bool IsContent { get; set; } = true;
}

public class WikiCategory
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    public long? ParentCategoryId { get; set; }
    public WikiCategory? ParentCategory { get; set; }
    public List<WikiCategory> SubCategories { get; set; } = [];
}
