using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc.Data;
using System.Text.Json.Serialization;

namespace mvc.Controllers;

public class GeoJsonFeature
{
    [JsonPropertyName("type")]
    public string Type => "Feature";

    [JsonPropertyName("geometry")]
    public GeoJsonGeometry Geometry { get; set; } = null!;

    [JsonPropertyName("properties")]
    public GeoJsonProperties Properties { get; set; } = null!;
}

public class GeoJsonGeometry
{
    [JsonPropertyName("type")]
    public string Type => "Point";

    [JsonPropertyName("coordinates")]
    public double[] Coordinates { get; set; } = [];  // [lon, lat]
}

public class GeoJsonProperties
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("typ")]
    public string? Typ { get; set; }
}

[Route("api/geojson")]
[ApiController]
public class GeoJsonController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public GeoJsonController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Liefert OSM-Punkte als GeoJSON FeatureCollection.
    /// Parameter: q (Namenssuche), typ (amenity/shop/tourism/…), bbox (lon_min,lat_min,lon_max,lat_max)
    /// Beispiele:
    ///   /api/geojson/poi?q=Schule
    ///   /api/geojson/poi?typ=restaurant
    ///   /api/geojson/poi?bbox=10.1,53.6,10.4,53.8
    ///   /api/geojson/poi?q=Café&typ=cafe&bbox=10.1,53.6,10.4,53.8
    /// </summary>
    [HttpGet("poi")]
    public async Task<IActionResult> Poi(
        [FromQuery] string? q,
        [FromQuery] string? typ,
        [FromQuery] string? bbox)
    {
        if (string.IsNullOrWhiteSpace(q) && string.IsNullOrWhiteSpace(typ) && string.IsNullOrWhiteSpace(bbox))
            return BadRequest("Mindestens ein Parameter erforderlich: q, typ oder bbox.");

        double[]? bboxVals = null;
        if (!string.IsNullOrWhiteSpace(bbox))
        {
            var parts = bbox.Split(',');
            if (parts.Length != 4 ||
                !parts.All(p => double.TryParse(p, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out _)))
                return BadRequest("bbox muss das Format lon_min,lat_min,lon_max,lat_max haben.");

            bboxVals = parts
                .Select(p => double.Parse(p, System.Globalization.CultureInfo.InvariantCulture))
                .ToArray();
        }

        var rohdaten = await AbfragenAsync(
            suche: string.IsNullOrWhiteSpace(q) ? null : $"%{q}%",
            typFilter: string.IsNullOrWhiteSpace(typ) ? null : typ,
            bboxVals: bboxVals);

        var featureCollection = new
        {
            type = "FeatureCollection",
            features = rohdaten.Select(p => new GeoJsonFeature
            {
                Geometry = new GeoJsonGeometry { Coordinates = [p.Lon, p.Lat] },
                Properties = new GeoJsonProperties { Name = p.Name, Typ = p.Typ }
            })
        };

        return Ok(featureCollection);
    }

    private Task<List<PoiErgebnis>> AbfragenAsync(string? suche, string? typFilter, double[]? bboxVals)
    {
        // Vier Kombinationen: mit/ohne bbox, mit/ohne typ
        if (bboxVals is not null && typFilter is not null && suche is not null)
            return _context.Database.SqlQuery<PoiErgebnis>($"""
                SELECT name,
                       COALESCE(amenity, shop, tourism, historic, leisure, place, highway) AS "Typ",
                       ST_Y(ST_Transform(way, 4326)) AS "Lat",
                       ST_X(ST_Transform(way, 4326)) AS "Lon"
                FROM planet_osm_point
                WHERE name IS NOT NULL
                  AND name ILIKE {suche}
                  AND (amenity={typFilter} OR shop={typFilter} OR tourism={typFilter}
                       OR historic={typFilter} OR leisure={typFilter} OR place={typFilter} OR highway={typFilter})
                  AND ST_Within(way, ST_Transform(
                      ST_MakeEnvelope({bboxVals[0]},{bboxVals[1]},{bboxVals[2]},{bboxVals[3]},4326),3857))
                ORDER BY name LIMIT 100
                """).ToListAsync();

        if (bboxVals is not null && typFilter is not null)
            return _context.Database.SqlQuery<PoiErgebnis>($"""
                SELECT name,
                       COALESCE(amenity, shop, tourism, historic, leisure, place, highway) AS "Typ",
                       ST_Y(ST_Transform(way, 4326)) AS "Lat",
                       ST_X(ST_Transform(way, 4326)) AS "Lon"
                FROM planet_osm_point
                WHERE name IS NOT NULL
                  AND (amenity={typFilter} OR shop={typFilter} OR tourism={typFilter}
                       OR historic={typFilter} OR leisure={typFilter} OR place={typFilter} OR highway={typFilter})
                  AND ST_Within(way, ST_Transform(
                      ST_MakeEnvelope({bboxVals[0]},{bboxVals[1]},{bboxVals[2]},{bboxVals[3]},4326),3857))
                ORDER BY name LIMIT 100
                """).ToListAsync();

        if (bboxVals is not null && suche is not null)
            return _context.Database.SqlQuery<PoiErgebnis>($"""
                SELECT name,
                       COALESCE(amenity, shop, tourism, historic, leisure, place, highway) AS "Typ",
                       ST_Y(ST_Transform(way, 4326)) AS "Lat",
                       ST_X(ST_Transform(way, 4326)) AS "Lon"
                FROM planet_osm_point
                WHERE name IS NOT NULL
                  AND name ILIKE {suche}
                  AND ST_Within(way, ST_Transform(
                      ST_MakeEnvelope({bboxVals[0]},{bboxVals[1]},{bboxVals[2]},{bboxVals[3]},4326),3857))
                ORDER BY name LIMIT 100
                """).ToListAsync();

        if (bboxVals is not null)
            return _context.Database.SqlQuery<PoiErgebnis>($"""
                SELECT name,
                       COALESCE(amenity, shop, tourism, historic, leisure, place, highway) AS "Typ",
                       ST_Y(ST_Transform(way, 4326)) AS "Lat",
                       ST_X(ST_Transform(way, 4326)) AS "Lon"
                FROM planet_osm_point
                WHERE name IS NOT NULL
                  AND ST_Within(way, ST_Transform(
                      ST_MakeEnvelope({bboxVals[0]},{bboxVals[1]},{bboxVals[2]},{bboxVals[3]},4326),3857))
                ORDER BY name LIMIT 100
                """).ToListAsync();

        if (typFilter is not null && suche is not null)
            return _context.Database.SqlQuery<PoiErgebnis>($"""
                SELECT name,
                       COALESCE(amenity, shop, tourism, historic, leisure, place, highway) AS "Typ",
                       ST_Y(ST_Transform(way, 4326)) AS "Lat",
                       ST_X(ST_Transform(way, 4326)) AS "Lon"
                FROM planet_osm_point
                WHERE name IS NOT NULL
                  AND name ILIKE {suche}
                  AND (amenity={typFilter} OR shop={typFilter} OR tourism={typFilter}
                       OR historic={typFilter} OR leisure={typFilter} OR place={typFilter} OR highway={typFilter})
                ORDER BY name LIMIT 100
                """).ToListAsync();

        if (typFilter is not null)
            return _context.Database.SqlQuery<PoiErgebnis>($"""
                SELECT name,
                       COALESCE(amenity, shop, tourism, historic, leisure, place, highway) AS "Typ",
                       ST_Y(ST_Transform(way, 4326)) AS "Lat",
                       ST_X(ST_Transform(way, 4326)) AS "Lon"
                FROM planet_osm_point
                WHERE name IS NOT NULL
                  AND (amenity={typFilter} OR shop={typFilter} OR tourism={typFilter}
                       OR historic={typFilter} OR leisure={typFilter} OR place={typFilter} OR highway={typFilter})
                ORDER BY name LIMIT 100
                """).ToListAsync();

        // nur suche
        return _context.Database.SqlQuery<PoiErgebnis>($"""
            SELECT name,
                   COALESCE(amenity, shop, tourism, historic, leisure, place, highway) AS "Typ",
                   ST_Y(ST_Transform(way, 4326)) AS "Lat",
                   ST_X(ST_Transform(way, 4326)) AS "Lon"
            FROM planet_osm_point
            WHERE name IS NOT NULL
              AND name ILIKE {suche}
            ORDER BY name LIMIT 100
            """).ToListAsync();
    }
}
