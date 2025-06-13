using System;

namespace LockerApp.Models.ViewModels
{
    public class InventoryOnsiteViewModel
    {
        public int InvlocId { get; set; }  // Primary key for database invloc_id field
        public string ItemId { get; set; } = null!;
        public string ItemName { get; set; } = null!;
        public string NewItemName { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public string NewDescription { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string LocationName { get; set; } = null!;
        public string? Berth { get; set; }
        public double OnHand { get; set; }
        public double NewOnHand { get; set; }
        public bool IsBillable { get; set; }
        public bool NewIsBillable { get; set; }
        public bool IsModified { get; set; }
    }
}


