using System.Reflection;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class UniverseDbContext : DbContext
{
    public UniverseDbContext(DbContextOptions<UniverseDbContext> options)
        : base(options)
    {
    }

    public DbSet<RouteEntity> Routes => Set<RouteEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        var entity = modelBuilder.Entity<RouteEntity>()
            .ToTable("routes")
            .HasNoKey();

        entity.Property(x => x.Origin).HasColumnName("origin");
        entity.Property(x => x.Destination).HasColumnName("destination");
        entity.Property(x => x.TravelTime).HasColumnName("travel_time");
        //modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
