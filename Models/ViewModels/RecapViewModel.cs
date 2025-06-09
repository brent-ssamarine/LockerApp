using System;

namespace AccessMigrationApp.Models.ViewModels
{
    public class RecapViewModel
    {
        public int InvLocId { get; set; }
        public string? Location { get; set; }
        public string? ItemId { get; set; }
        public string? ItemName { get; set; }
        public double? OnHand { get; set; }
        public string? Description { get; set; }
        public string? LocationType { get; set; }
        public DateTime? TransferDate { get; set; }
        public double? Quantity { get; set; }
        public int Consumed { get; set; }
        public string? InspectedBy { get; set; }
        
        // UI-specific properties
        public bool IsSelected { get; set; }
        public bool IsModified { get; set; }
        
        // Display properties
        public string FormattedTransferDate => TransferDate?.ToString("MM/dd/yyyy") ?? "";
        public string FormattedOnHand => OnHand?.ToString("F2") ?? "0.00";
        public string FormattedQuantity => Quantity?.ToString("F2") ?? "0.00";
        public string ConsumedDisplay => Consumed == 1 ? "Yes" : "No";
        
        // Constructor
        public RecapViewModel()
        {
            IsSelected = false;
            IsModified = false;
        }
        
        // Constructor from Recap model
        public RecapViewModel(AccessMigrationApp.Models.LockerDB.Recap recap)
        {
            InvLocId = recap.invlocId;
            Location = recap.Location;
            ItemId = recap.ItemId;
            ItemName = recap.ItemName;
            OnHand = recap.OnHand;
            Description = recap.Description;
            LocationType = recap.LocationType;
            TransferDate = recap.TransferDate;
            Quantity = recap.Quantity;
            Consumed = recap.Consumed;
            InspectedBy = recap.InspectedBy;
            
            IsSelected = false;
            IsModified = false;
        }
        
        // Method to convert back to Recap model
        public AccessMigrationApp.Models.LockerDB.Recap ToModel()
        {
            return new AccessMigrationApp.Models.LockerDB.Recap
            {
                invlocId = InvLocId,
                Location = Location,
                ItemId = ItemId,
                ItemName = ItemName,
                OnHand = OnHand,
                Description = Description,
                LocationType = LocationType,
                TransferDate = TransferDate,
                Quantity = Quantity,
                Consumed = Consumed,
                InspectedBy = InspectedBy
            };
        }
    }
}
