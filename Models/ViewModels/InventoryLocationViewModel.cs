using AccessMigrationApp.Models.LockerDB;

namespace AccessMigrationApp.Models.ViewModels
{
    public class InventoryLocationViewModel
    {
        public string ItemId { get; set; } = null!;
        public string ItemName { get; set; } = null!;
        public string NewItemName { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public string NewDescription { get; set; } = string.Empty;
        public string Type { get; set; } = null!;
        public string Class { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string LocationName { get; set; } = null!;
        public string LocationType { get; set; } = string.Empty;
        public string? Berth { get; set; }
        public string? Gear { get; set; }
        public string? Cargo { get; set; }
        public double OnHand { get; set; }
        public double NewOnHand { get; set; }
        public bool IsBillable { get; set; }
        public bool NewIsBillable { get; set; }
        public bool IsModified { get; set; }
        public double Issued { get; set; }
        public double? StandardCost { get; set; }
        public string? Process { get; set; }
        public string? TestClass { get; set; }
        public DateTime? Visual { get; set; }
        public DateTime? Thorough { get; set; }
        public bool IsActive { get; set; }
    }
} 