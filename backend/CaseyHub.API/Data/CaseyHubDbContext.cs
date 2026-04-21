using CaseyHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseyHub.API.Data;

public class CaseyHubDbContext(DbContextOptions<CaseyHubDbContext> options) : DbContext(options)
{
    public DbSet<Permit> Permits => Set<Permit>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permit>(entity =>
        {
            entity.HasKey(permit => permit.ApplicationNumber);

            entity.Property(permit => permit.ApplicationNumber)
                .HasMaxLength(100);

            entity.Property(permit => permit.ApplicationCategory)
                .HasMaxLength(200);

            entity.Property(permit => permit.Status)
                .HasMaxLength(100);

            entity.Property(permit => permit.StageDecision)
                .HasMaxLength(200);

            entity.OwnsOne(p => p.Location, location =>
            {
                location.Property(l => l.RawAddress).HasColumnName("RawAddress");
                location.Property(l => l.HouseNumber).HasColumnName("HouseNumber"); 
                location.Property(l => l.Street).HasColumnName("Street");           
                location.Property(l => l.Suburb).HasColumnName("Suburb");
                location.Property(l => l.Municipality).HasColumnName("Municipality");
                location.Property(l => l.State).HasColumnName("State");
                location.Property(l => l.Postcode).HasColumnName("Postcode");
                location.Property(l => l.Latitude).HasColumnName("Latitude");
                location.Property(l => l.Longitude).HasColumnName("Longitude");
            });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);

            entity.HasIndex(user => user.Email)
                .IsUnique();

            entity.Property(user => user.Name)
                .HasMaxLength(150);

            entity.Property(user => user.Email)
                .HasMaxLength(320);

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(1000);
        });
    }
}
