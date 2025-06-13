using LockerApp.Models.LockerDB;

namespace LockerApp.Models.ViewModels
{
    public class MaterialListViewModel
    {
        public int LocationId { get; set; }
        public string? FromLocationName { get; set; }
        public string? Description { get; set; }
        public string? Berth { get; set; }
        public DateTime? StartDate { get; set; }        public string? ItemName { get; set; }
        public double? Quantity { get; set; }
        public string? ToLocationName { get; set; }
        public string? InvType { get; set; }

        public static MaterialListViewModel FromModel(MaterialList model)
        {
            return new MaterialListViewModel
            {
                LocationId = model.LocationId,
                FromLocationName = model.FromLocationName,
                Description = model.Description,
                Berth = model.Berth,
                StartDate = model.StartDate,
                ItemName = model.ItemName,
                Quantity = model.Quantity,
                ToLocationName = model.ToLocationName,
                InvType = model.InvType
            };
        }

        public MaterialList ToModel()
        {
            return new MaterialList
            {
                LocationId = LocationId,
                FromLocationName = FromLocationName,
                Description = Description,
                Berth = Berth,
                StartDate = StartDate,
                ItemName = ItemName,
                Quantity = Quantity,
                ToLocationName = ToLocationName,
                InvType = InvType
            };
        }
    }
}
