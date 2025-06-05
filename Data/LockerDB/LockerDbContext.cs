using Microsoft.EntityFrameworkCore;
using AccessMigrationApp.Models.LockerDB;

namespace AccessMigrationApp.Data.LockerDB;

public class LockerDbContext : DbContext
{
    public LockerDbContext(DbContextOptions<LockerDbContext> options)
        : base(options)
    {
    }

    public DbSet<BerthMaster> BerthMasters { get; set; }
    public DbSet<Berth> Berths { get; set; }
    public DbSet<Certificate> Certificates { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryLocation> InventoryLocations { get; set; }
    public DbSet<InventoryTransfer> InventoryTransfers { get; set; }
    public DbSet<JobItem> JobItems { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Line> Lines { get; set; }
    public DbSet<LocationType> LocationTypes { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<LockerInventory> LockerInventories { get; set; }
    public DbSet<InventoryOnsite> InventoryOnsites { get; set; }
    public DbSet<TestClass> TestClasses { get; set; }
    public DbSet<TwoWeek> TwoWeeks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("dbo");

        // Configure any table-specific settings here
        modelBuilder.Entity<LockerInventory>()
            .ToTable("LOCKERINV");
    }
}
