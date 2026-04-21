using JukeboxDataManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace JukeboxDataManager.Data;

public class JukeboxDbContext : DbContext
{
    public JukeboxDbContext(DbContextOptions<JukeboxDbContext> options) : base(options) { }

    public DbSet<Song> Songs => Set<Song>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Album> Albums => Set<Album>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(200);
            entity.Property(a => a.Bio).HasMaxLength(2000);
            entity.HasMany(a => a.Songs).WithOne(s => s.Artist).HasForeignKey(s => s.ArtistId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(a => a.Albums).WithOne(al => al.Artist).HasForeignKey(al => al.ArtistId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Album>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Title).IsRequired().HasMaxLength(200);
            entity.HasMany(a => a.Songs).WithOne(s => s.Album).HasForeignKey(s => s.AlbumId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Song>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Title).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Genre).HasMaxLength(100);
        });
    }
}
