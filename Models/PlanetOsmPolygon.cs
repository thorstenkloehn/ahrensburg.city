using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace ahrensburg.city.Models;

public partial class PlanetOsmPolygon
{
    public long? OsmId { get; set; }

    public string? Access { get; set; }

    public string? AddrHousename { get; set; }

    public string? AddrHousenumber { get; set; }

    public string? AddrInterpolation { get; set; }

    public string? AdminLevel { get; set; }

    public string? Aerialway { get; set; }

    public string? Aeroway { get; set; }

    public string? Amenity { get; set; }

    public string? Area { get; set; }

    public string? Barrier { get; set; }

    public string? Bicycle { get; set; }

    public string? Brand { get; set; }

    public string? Bridge { get; set; }

    public string? Boundary { get; set; }

    public string? Building { get; set; }

    public string? Construction { get; set; }

    public string? Covered { get; set; }

    public string? Culvert { get; set; }

    public string? Cutting { get; set; }

    public string? Denomination { get; set; }

    public string? Disused { get; set; }

    public string? Embankment { get; set; }

    public string? Foot { get; set; }

    public string? GeneratorSource { get; set; }

    public string? Harbour { get; set; }

    public string? Highway { get; set; }

    public string? Historic { get; set; }

    public string? Horse { get; set; }

    public string? Intermittent { get; set; }

    public string? Junction { get; set; }

    public string? Landuse { get; set; }

    public string? Layer { get; set; }

    public string? Leisure { get; set; }

    public string? Lock { get; set; }

    public string? ManMade { get; set; }

    public string? Military { get; set; }

    public string? Motorcar { get; set; }

    public string? Name { get; set; }

    public string? Natural { get; set; }

    public string? Office { get; set; }

    public string? Oneway { get; set; }

    public string? Operator { get; set; }

    public string? Place { get; set; }

    public string? Population { get; set; }

    public string? Power { get; set; }

    public string? PowerSource { get; set; }

    public string? PublicTransport { get; set; }

    public string? Railway { get; set; }

    public string? Ref { get; set; }

    public string? Religion { get; set; }

    public string? Route { get; set; }

    public string? Service { get; set; }

    public string? Shop { get; set; }

    public string? Sport { get; set; }

    public string? Surface { get; set; }

    public string? Toll { get; set; }

    public string? Tourism { get; set; }

    public string? TowerType { get; set; }

    public string? Tracktype { get; set; }

    public string? Tunnel { get; set; }

    public string? Water { get; set; }

    public string? Waterway { get; set; }

    public string? Wetland { get; set; }

    public string? Width { get; set; }

    public string? Wood { get; set; }

    public int? ZOrder { get; set; }

    public float? WayArea { get; set; }

    public Dictionary<string, string>? Tags { get; set; }

    public Geometry? Way { get; set; }
}
