using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc.Data;

namespace mvc.Controllers;

public class PoiErgebnis
{
    public string Name { get; set; } = string.Empty;
    public string? Typ { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
}

[Route("api/poi")]
[ApiController]
public class PoiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PoiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Suche([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return BadRequest("Suchbegriff zu kurz (min. 2 Zeichen).");

        var suche = $"%{q}%";

        var ergebnisse = await _context.Database
            .SqlQuery<PoiErgebnis>($"""
                SELECT name,
                       COALESCE(amenity, shop, tourism, historic, leisure, place, highway) AS "Typ",
                       ST_Y(ST_Transform(way, 4326)) AS "Lat",
                       ST_X(ST_Transform(way, 4326)) AS "Lon"
                FROM planet_osm_point
                WHERE name IS NOT NULL
                  AND name ILIKE {suche}
                ORDER BY name
                LIMIT 20
                """)
            .ToListAsync();

        return Ok(ergebnisse);
    }
}
