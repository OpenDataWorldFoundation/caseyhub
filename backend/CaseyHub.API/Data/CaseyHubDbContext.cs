using CaseyHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaseyHub.API.Data;

public class CaseyHubDbContext(DbContextOptions<CaseyHubDbContext> options) : DbContext(options)
{
    public DbSet<Permit> Permits => Set<Permit>();

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
    }
}
