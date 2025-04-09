using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ahrensburg.city.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<PlanetOsmLine> PlanetOsmLines { get; set; }

    public virtual DbSet<PlanetOsmPoint> PlanetOsmPoints { get; set; }

    public virtual DbSet<PlanetOsmPolygon> PlanetOsmPolygons { get; set; }

    public virtual DbSet<PlanetOsmRoad> PlanetOsmRoads { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=thorsten;Username=thorsten;Password=Test", x => x.UseNetTopologySuite());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("hstore")
            .HasPostgresExtension("postgis")
            .HasPostgresExtension("vector");

        modelBuilder.Entity<PlanetOsmLine>(entity =>
        {
            entity.HasIndex(e => e.Way, "planet_osm_line_way_idx")
                .HasMethod("gist")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");
        });

        modelBuilder.Entity<PlanetOsmPoint>(entity =>
        {
            entity.HasIndex(e => e.Way, "planet_osm_point_way_idx")
                .HasMethod("gist")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");
        });

        modelBuilder.Entity<PlanetOsmPolygon>(entity =>
        {
            entity.HasIndex(e => e.Way, "planet_osm_polygon_way_idx")
                .HasMethod("gist")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");
        });

        modelBuilder.Entity<PlanetOsmRoad>(entity =>
        {
            entity.HasIndex(e => e.Way, "planet_osm_roads_way_idx")
                .HasMethod("gist")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
