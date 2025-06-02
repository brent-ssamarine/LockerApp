using System.ComponentModel.DataAnnotations;
using AccessMigrationApp.Models.LockerDB;

namespace AccessMigrationApp.Models.ViewModels
{
    public class InventoryViewModel
    {
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public short? Billable { get; set; }
        public string? Process { get; set; }
        public string InvType { get; set; } = null!;
        public string Class { get; set; } = null!;
        public double? StandardCost { get; set; }
        public int? PreferredVendor { get; set; }
        public short? Accumulate { get; set; }
        public short? Active { get; set; }
        public string? TestClass { get; set; }
        public DateTime? Visual { get; set; }
        public DateTime? Thorough { get; set; }    public static InventoryViewModel FromModel(AccessMigrationApp.Models.LockerDB.Inventory model)
    {
        return new InventoryViewModel
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Billable = model.Billable,
            Process = model.Process,
            InvType = model.InvType,
            Class = model.Class,
            StandardCost = model.StandardCost,
            PreferredVendor = model.PreferredVendor,
            Accumulate = model.Accumulate,
            Active = model.Active,
            TestClass = model.TestClass,
            Visual = model.Visual,
            Thorough = model.Thorough
        };
    }
}
}
