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

            entity.Property(permit => permit.Address)
                .HasMaxLength(500);
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
