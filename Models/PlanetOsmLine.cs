using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace ahrensburg.city.Models;

[Keyless]
[Table("planet_osm_line")]
public partial class PlanetOsmLine
{
    [Column("osm_id")]
    public long? OsmId { get; set; }

    [Column("access")]
    public string? Access { get; set; }

    [Column("addr:housename")]
    public string? AddrHousename { get; set; }

    [Column("addr:housenumber")]
    public string? AddrHousenumber { get; set; }

    [Column("addr:interpolation")]
    public string? AddrInterpolation { get; set; }

    [Column("admin_level")]
    public string? AdminLevel { get; set; }

    [Column("aerialway")]
    public string? Aerialway { get; set; }

    [Column("aeroway")]
    public string? Aeroway { get; set; }

    [Column("amenity")]
    public string? Amenity { get; set; }

    [Column("area")]
    public string? Area { get; set; }

    [Column("barrier")]
    public string? Barrier { get; set; }

    [Column("bicycle")]
    public string? Bicycle { get; set; }

    [Column("brand")]
    public string? Brand { get; set; }

    [Column("bridge")]
    public string? Bridge { get; set; }

    [Column("boundary")]
    public string? Boundary { get; set; }

    [Column("building")]
    public string? Building { get; set; }

    [Column("construction")]
    public string? Construction { get; set; }

    [Column("covered")]
    public string? Covered { get; set; }

    [Column("culvert")]
    public string? Culvert { get; set; }

    [Column("cutting")]
    public string? Cutting { get; set; }

    [Column("denomination")]
    public string? Denomination { get; set; }

    [Column("disused")]
    public string? Disused { get; set; }

    [Column("embankment")]
    public string? Embankment { get; set; }

    [Column("foot")]
    public string? Foot { get; set; }

    [Column("generator:source")]
    public string? GeneratorSource { get; set; }

    [Column("harbour")]
    public string? Harbour { get; set; }

    [Column("highway")]
    public string? Highway { get; set; }

    [Column("historic")]
    public string? Historic { get; set; }

    [Column("horse")]
    public string? Horse { get; set; }

    [Column("intermittent")]
    public string? Intermittent { get; set; }

    [Column("junction")]
    public string? Junction { get; set; }

    [Column("landuse")]
    public string? Landuse { get; set; }

    [Column("layer")]
    public string? Layer { get; set; }

    [Column("leisure")]
    public string? Leisure { get; set; }

    [Column("lock")]
    public string? Lock { get; set; }

    [Column("man_made")]
    public string? ManMade { get; set; }

    [Column("military")]
    public string? Military { get; set; }

    [Column("motorcar")]
    public string? Motorcar { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("natural")]
    public string? Natural { get; set; }

    [Column("office")]
    public string? Office { get; set; }

    [Column("oneway")]
    public string? Oneway { get; set; }

    [Column("operator")]
    public string? Operator { get; set; }

    [Column("place")]
    public string? Place { get; set; }

    [Column("population")]
    public string? Population { get; set; }

    [Column("power")]
    public string? Power { get; set; }

    [Column("power_source")]
    public string? PowerSource { get; set; }

    [Column("public_transport")]
    public string? PublicTransport { get; set; }

    [Column("railway")]
    public string? Railway { get; set; }

    [Column("ref")]
    public string? Ref { get; set; }

    [Column("religion")]
    public string? Religion { get; set; }

    [Column("route")]
    public string? Route { get; set; }

    [Column("service")]
    public string? Service { get; set; }

    [Column("shop")]
    public string? Shop { get; set; }

    [Column("sport")]
    public string? Sport { get; set; }

    [Column("surface")]
    public string? Surface { get; set; }

    [Column("toll")]
    public string? Toll { get; set; }

    [Column("tourism")]
    public string? Tourism { get; set; }

    [Column("tower:type")]
    public string? TowerType { get; set; }

    [Column("tracktype")]
    public string? Tracktype { get; set; }

    [Column("tunnel")]
    public string? Tunnel { get; set; }

    [Column("water")]
    public string? Water { get; set; }

    [Column("waterway")]
    public string? Waterway { get; set; }

    [Column("wetland")]
    public string? Wetland { get; set; }

    [Column("width")]
    public string? Width { get; set; }

    [Column("wood")]
    public string? Wood { get; set; }

    [Column("z_order")]
    public int? ZOrder { get; set; }

    [Column("way_area")]
    public float? WayArea { get; set; }

    [Column("tags")]
    public Dictionary<string, string>? Tags { get; set; }

    [Column("way", TypeName = "geometry(LineString,3857)")]
    public LineString? Way { get; set; }
}
