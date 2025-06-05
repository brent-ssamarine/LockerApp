using System;

namespace AccessMigrationApp.Models.ViewModels;

public class InventoryOnsiteViewModel
{
    public string? Item { get; set; }
    public string? ItemName { get; set; }
    public string? Description { get; set; }
    public string? InvType { get; set; }
    public string? Class { get; set; }
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public double? OnHand { get; set; }
    public double? Issued { get; set; }
    public bool IsBillable => Billable == 1;
    public short? Billable { get; set; }
    public bool IsActive => Active == 1;
    public short? Active { get; set; }
    public bool IsFinished => Finished == 1;
    public short? Finished { get; set; }
}
