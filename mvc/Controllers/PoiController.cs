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
