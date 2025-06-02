using Microsoft.EntityFrameworkCore;
using AccessMigrationApp.Models.BCMEA;

namespace AccessMigrationApp.Data.BCMEA;

public class BCMEAContext : DbContext
{
    public BCMEAContext(DbContextOptions<BCMEAContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("dbo");

        // Make all entities read-only
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.AddAnnotation("ReadOnly", true);
            var properties = entity.GetProperties();
            entity.SetTableName(entity.GetTableName()?.ToLower());
            foreach (var property in properties)
            {
                property.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
            }
        }
    }

    public override int SaveChanges()
    {
        PreventChangesSave();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        PreventChangesSave();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void PreventChangesSave()
    {
        var changedEntities = ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged)
            .ToList();

        if (changedEntities.Any())
        {
            throw new InvalidOperationException("The BCMEA database is read-only. Changes are not allowed.");
        }
    }
}
