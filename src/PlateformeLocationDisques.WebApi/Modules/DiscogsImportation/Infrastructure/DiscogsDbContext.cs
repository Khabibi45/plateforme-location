using Microsoft.EntityFrameworkCore;
using PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Domain;

namespace PlateformeLocationDisques.WebApi.Modules.DiscogsImportation.Infrastructure;

/// <summary>
/// DbContext for the Discogs Importation module.
/// Isolates the persistence layer for this module following modular monolith principles.
/// </summary>
public class DiscogsDbContext : DbContext
{
    public DiscogsDbContext(DbContextOptions<DiscogsDbContext> options) : base(options)
    {
    }

    public DbSet<MasterRelease> MasterReleases => Set<MasterRelease>();
    public DbSet<Release> Releases => Set<Release>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set schema for module isolation
        modelBuilder.HasDefaultSchema("discogs");

        ConfigureMasterRelease(modelBuilder);
        ConfigureRelease(modelBuilder);
    }

    private static void ConfigureMasterRelease(ModelBuilder modelBuilder)
    {
        var masterRelease = modelBuilder.Entity<MasterRelease>();

        masterRelease.HasKey(m => m.Id);

        masterRelease.Property(m => m.Id)
            .HasConversion(
                ulid => ulid.ToString(),
                str => ByteAether.Ulid.Ulid.Parse(str))
            .IsRequired();

        // Index on Discogs ID for fast lookups
        masterRelease.HasIndex(m => m.DiscogsId)
            .IsUnique();

        masterRelease.Property(m => m.Title).IsRequired().HasMaxLength(500);
        masterRelease.Property(m => m.Status).HasMaxLength(50);
        masterRelease.Property(m => m.DataQuality).HasMaxLength(100);
        masterRelease.Property(m => m.Uri).HasMaxLength(500);
        masterRelease.Property(m => m.ResourceUrl).HasMaxLength(500);

        // Store arrays as JSON
        masterRelease.Property(m => m.Genres)
            .HasColumnType("jsonb");

        masterRelease.Property(m => m.Styles)
            .HasColumnType("jsonb");

        // Owned types for complex objects
        masterRelease.OwnsMany(m => m.Artists, artist =>
        {
            artist.Property(a => a.Id)
                .HasConversion(
                    ulid => ulid.ToString(),
                    str => ByteAether.Ulid.Ulid.Parse(str));
            artist.Property(a => a.Name).IsRequired().HasMaxLength(500);
            artist.Property(a => a.Anv).HasMaxLength(500);
            artist.Property(a => a.ResourceUrl).HasMaxLength(500);
        });

        masterRelease.OwnsMany(m => m.Tracklist, track =>
        {
            track.Property(t => t.Id)
                .HasConversion(
                    ulid => ulid.ToString(),
                    str => ByteAether.Ulid.Ulid.Parse(str));
            track.Property(t => t.Title).IsRequired().HasMaxLength(500);
            track.Property(t => t.Position).HasMaxLength(50);
            track.Property(t => t.Type).HasMaxLength(50);

            track.OwnsMany(t => t.Artists, artist =>
            {
                artist.Property(a => a.Id)
                    .HasConversion(
                        ulid => ulid.ToString(),
                        str => ByteAether.Ulid.Ulid.Parse(str));
                artist.Property(a => a.Name).IsRequired().HasMaxLength(500);
            });

            track.OwnsMany(t => t.ExtraArtists, artist =>
            {
                artist.Property(a => a.Id)
                    .HasConversion(
                        ulid => ulid.ToString(),
                        str => ByteAether.Ulid.Ulid.Parse(str));
                artist.Property(a => a.Name).IsRequired().HasMaxLength(500);
            });

            track.OwnsMany(t => t.SubTracks, subTrack =>
            {
                subTrack.Property(st => st.Id)
                    .HasConversion(
                        ulid => ulid.ToString(),
                        str => ByteAether.Ulid.Ulid.Parse(str));
                subTrack.Property(st => st.Title).IsRequired().HasMaxLength(500);
            });
        });

        masterRelease.OwnsMany(m => m.Images, image =>
        {
            image.Property(i => i.Id)
                .HasConversion(
                    ulid => ulid.ToString(),
                    str => ByteAether.Ulid.Ulid.Parse(str));
            image.Property(i => i.Type).HasMaxLength(50);
            image.Property(i => i.Uri).HasMaxLength(1000);
            image.Property(i => i.Uri150).HasMaxLength(1000);
        });

        masterRelease.OwnsMany(m => m.Videos, video =>
        {
            video.Property(v => v.Id)
                .HasConversion(
                    ulid => ulid.ToString(),
                    str => ByteAether.Ulid.Ulid.Parse(str));
            video.Property(v => v.Uri).HasMaxLength(1000);
            video.Property(v => v.Title).HasMaxLength(500);
        });

        masterRelease.OwnsOne(m => m.Community, community =>
        {
            community.Property(c => c.Id)
                .HasConversion(
                    ulid => ulid.ToString(),
                    str => ByteAether.Ulid.Ulid.Parse(str));
            community.Property(c => c.Status).HasMaxLength(50);
            community.Property(c => c.DataQuality).HasMaxLength(100);

            community.OwnsMany(c => c.Contributors, contributor =>
            {
                contributor.Property(co => co.Id)
                    .HasConversion(
                        ulid => ulid.ToString(),
                        str => ByteAether.Ulid.Ulid.Parse(str));
                contributor.Property(co => co.Username).HasMaxLength(200);
            });
        });

        // Relationships
        masterRelease.HasMany(m => m.Releases)
            .WithOne(r => r.MasterRelease)
            .HasForeignKey(r => r.MasterReleaseId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureRelease(ModelBuilder modelBuilder)
    {
        var release = modelBuilder.Entity<Release>();

        release.HasKey(r => r.Id);

        release.Property(r => r.Id)
            .HasConversion(
                ulid => ulid.ToString(),
                str => ByteAether.Ulid.Ulid.Parse(str))
            .IsRequired();

        release.Property(r => r.MasterReleaseId)
            .HasConversion(
                ulid => ulid.HasValue ? ulid.Value.ToString() : null,
                str => str != null ? ByteAether.Ulid.Ulid.Parse(str) : null);

        // Index on Discogs ID for fast lookups
        release.HasIndex(r => r.DiscogsId)
            .IsUnique();

        release.Property(r => r.Title).IsRequired().HasMaxLength(500);
        release.Property(r => r.Status).HasMaxLength(50);
        release.Property(r => r.DataQuality).HasMaxLength(100);
        release.Property(r => r.Country).HasMaxLength(100);

        // Store arrays as JSON
        release.Property(r => r.Genres)
            .HasColumnType("jsonb");

        release.Property(r => r.Styles)
            .HasColumnType("jsonb");

        // Owned types similar to MasterRelease
        release.OwnsMany(r => r.Artists, artist =>
        {
            artist.Property(a => a.Id)
                .HasConversion(
                    ulid => ulid.ToString(),
                    str => ByteAether.Ulid.Ulid.Parse(str));
            artist.Property(a => a.Name).IsRequired().HasMaxLength(500);
        });

        release.OwnsMany(r => r.Labels, label =>
        {
            label.Property(l => l.Id)
                .HasConversion(
                    ulid => ulid.ToString(),
                    str => ByteAether.Ulid.Ulid.Parse(str));
            label.Property(l => l.Name).IsRequired().HasMaxLength(500);
            label.Property(l => l.CatalogNumber).HasMaxLength(200);
        });

        release.OwnsMany(r => r.Formats, format =>
        {
            format.Property(f => f.Id)
                .HasConversion(
                    ulid => ulid.ToString(),
                    str => ByteAether.Ulid.Ulid.Parse(str));
            format.Property(f => f.Name).IsRequired().HasMaxLength(200);
            format.Property(f => f.Descriptions).HasColumnType("jsonb");
        });

        release.OwnsMany(r => r.Tracklist, track =>
        {
            track.Property(t => t.Id)
                .HasConversion(
                    ulid => ulid.ToString(),
                    str => ByteAether.Ulid.Ulid.Parse(str));
            track.Property(t => t.Title).IsRequired().HasMaxLength(500);

            track.OwnsMany(t => t.Artists, artist =>
            {
                artist.Property(a => a.Id)
                    .HasConversion(
                        ulid => ulid.ToString(),
                        str => ByteAether.Ulid.Ulid.Parse(str));
                artist.Property(a => a.Name).IsRequired().HasMaxLength(500);
            });

            track.OwnsMany(t => t.ExtraArtists, artist =>
            {
                artist.Property(a => a.Id)
                    .HasConversion(
                        ulid => ulid.ToString(),
                        str => ByteAether.Ulid.Ulid.Parse(str));
                artist.Property(a => a.Name).IsRequired().HasMaxLength(500);
            });

            track.OwnsMany(t => t.SubTracks, subTrack =>
            {
                subTrack.Property(st => st.Id)
                    .HasConversion(
                        ulid => ulid.ToString(),
                        str => ByteAether.Ulid.Ulid.Parse(str));
                subTrack.Property(st => st.Title).IsRequired().HasMaxLength(500);
            });
        });

        release.OwnsMany(r => r.Identifiers, identifier =>
        {
            identifier.Property(i => i.Id)
                .HasConversion(
                    ulid => ulid.ToString(),
                    str => ByteAether.Ulid.Ulid.Parse(str));
            identifier.Property(i => i.Type).HasMaxLength(100);
            identifier.Property(i => i.Value).HasMaxLength(500);
        });

        release.OwnsMany(r => r.Images, image =>
        {
            image.Property(i => i.Id)
                .HasConversion(
                    ulid => ulid.ToString(),
                    str => ByteAether.Ulid.Ulid.Parse(str));
            image.Property(i => i.Uri).HasMaxLength(1000);
        });

        release.OwnsMany(r => r.Videos, video =>
        {
            video.Property(v => v.Id)
                .HasConversion(
                    ulid => ulid.ToString(),
                    str => ByteAether.Ulid.Ulid.Parse(str));
            video.Property(v => v.Uri).HasMaxLength(1000);
        });

        release.OwnsOne(r => r.Community, community =>
        {
            community.Property(c => c.Id)
                .HasConversion(
                    ulid => ulid.ToString(),
                    str => ByteAether.Ulid.Ulid.Parse(str));
            community.Property(c => c.Status).HasMaxLength(50);

            community.OwnsMany(c => c.Contributors, contributor =>
            {
                contributor.Property(co => co.Id)
                    .HasConversion(
                        ulid => ulid.ToString(),
                        str => ByteAether.Ulid.Ulid.Parse(str));
                contributor.Property(co => co.Username).HasMaxLength(200);
            });
        });
    }
}
