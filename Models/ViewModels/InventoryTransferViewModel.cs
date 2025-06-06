using System;

namespace AccessMigrationApp.Models.ViewModels;

public class InventoryTransferViewModel
{
    public int Id { get; set; }
    public string? ItemId { get; set; }
    public string? ItemName { get; set; }
    public string? ItemDescription { get; set; }
    public int? FromLocation { get; set; }
    public int? ToLocation { get; set; }
    public string? Company { get; set; }
    public string? Job { get; set; }
    public short? IssueValue { get; set; }
    public string? TakenFrom { get; set; }
    public DateTime? TransferDate { get; set; }
    public double? Quantity { get; set; }
    public double? CostPer { get; set; }
    public string? PONumber { get; set; }
    public double? Locker { get; set; }
    public double? OnHand { get; set; }
    public string? InspectedBy { get; set; }
}
