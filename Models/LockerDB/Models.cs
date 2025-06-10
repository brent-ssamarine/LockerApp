using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccessMigrationApp.Models.LockerDB;

[Keyless]
[Table("berth_master")]
public class BerthMaster
{
    [Column("be_name")]
    public string? Name { get; set; }
    
    [Column("be_port")]
    public string? Port { get; set; }
}

[Table("berths")]
public class Berth
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = null!;
    
    [Column("name")]
    public string? Name { get; set; }
    
    [Column("port")]
    public string? Port { get; set; }
    
    [Column("is_archived")]
    public bool IsArchived { get; set; }
}

[Table("certificates")]
public class Certificate
{
    [Key]
    [Column("certificate")]
    public string Id { get; set; } = null!;
    
    [Column("pass_date")]
    public DateTime? PassDate { get; set; }
}

[Table("inventory")]
public class Inventory
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = null!;
    
    [Column("name")]
    public string? Name { get; set; }
    
    [Column("description")]
    public string? Description { get; set; }
    
    [Column("billable")]
    public short? Billable { get; set; }
    
    [Column("process")]
    public string? Process { get; set; }
    
    [Column("inv_type")]
    public string InvType { get; set; } = null!;
    
    [Column("class")]
    public string Class { get; set; } = null!;
    
    [Column("standardcost")]
    public double? StandardCost { get; set; }
    
    [Column("preferred_vendor")]
    public int? PreferredVendor { get; set; }
    
    [Column("accumulate")]
    public short? Accumulate { get; set; }
    
    [Column("active")]
    public short? Active { get; set; }
    
    [Column("testclass")]
    public string? TestClass { get; set; }
    
    [Column("visual")]
    public DateTime? Visual { get; set; }
    
    [Column("thorough")]
    public DateTime? Thorough { get; set; }

    public ICollection<InventoryLocation>? InventoryLocations { get; set; }
    public ICollection<InventoryTransfer>? InventoryTransfers { get; set; }
    public ICollection<JobItem>? JobItems { get; set; }
}

[Table("inventory_locations")]
public class InventoryLocation
{
    [Key]
    [Column("invloc_id")]
    public int Id { get; set; }
    
    [Column("item")]
    public string? ItemId { get; set; }
    
    [Column("item_name")]
    public string? ItemName { get; set; }
    
    [Column("location")]
    public int? LocationId { get; set; }
    
    [Column("description")]
    public string? Description { get; set; }
    
    [Column("issued")]
    public double? Issued { get; set; }
    
    [Column("onhand")]
    public double? OnHand { get; set; }
    
    [Column("billable")]
    public short? Billable { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Inventory? Inventory { get; set; }

    [ForeignKey(nameof(LocationId))]
    public Location? Location { get; set; }
}

[Table("inventory_transfers")]
public class InventoryTransfer
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("item")]
    public string? ItemId { get; set; }
    
    [Column("item_name")]
    public string? ItemName { get; set; }
    
    [Column("item_desc")]
    public string? ItemDescription { get; set; }
    
    [Column("from_location")]
    public int? FromLocation { get; set; }
    
    [Column("to_location")]
    public int? ToLocation { get; set; }
    
    [Column("company")]
    public string? Company { get; set; }
    
    [Column("job")]
    public string? Job { get; set; }
    
    [Column("issue_val")]
    public short? IssueValue { get; set; }
    
    [Column("taken_from")]
    public string? TakenFrom { get; set; }
    
    [Column("transfer_date")]
    public DateTime? TransferDate { get; set; }
    
    [Column("quantity")]
    public double? Quantity { get; set; }
    
    [Column("costper")]
    public double? CostPer { get; set; }
    
    [Column("ponum")]
    public string? PONumber { get; set; }
    
    [Column("locker")]
    public double? Locker { get; set; }
    
    [Column("onhand")]
    public double? OnHand { get; set; }
    
    [Column("inspected_by")]
    public string? InspectedBy { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Inventory? Inventory { get; set; }
}

[Table("job_item")]
public class JobItem
{
    [Key]
    [Column("recnum")]
    public int Id { get; set; }
    
    [Column("company")]
    public string Company { get; set; } = null!;
    
    [Column("job_class")]
    public string JobClass { get; set; } = null!;
    
    [Column("inv_item")]
    public string? InvItem { get; set; }
    
    [Column("inv_note")]
    public string? InvNote { get; set; }
    
    [Column("inv_name")]
    public string? InvName { get; set; }
    
    [Column("default_qty")]
    public double? DefaultQuantity { get; set; }
    
    [Column("issue_qty")]
    public double? IssueQuantity { get; set; }
    
    [Column("billable")]
    public bool? Billable { get; set; }

    [ForeignKey(nameof(InvItem))]
    public Inventory? Inventory { get; set; }
}

[Table("jobs")]
public class Job
{
    [Key]
    [Column("name")]
    public string Name { get; set; } = null!;
    
    [Column("company")]
    public string Company { get; set; } = null!;

    public ICollection<JobItem>? JobItems { get; set; }
}

[Table("line")]
public class Line
{
    [Key]
    [Column("id")]
    [StringLength(6)]
    public string Id { get; set; } = null!;
    
    [Column("name")]
    [StringLength(30)]
    public string? Name { get; set; }
    
    [Column("sharehold_grp")]
    [StringLength(8)]
    public string? ShareholdGroup { get; set; }
    
    [Column("comm_group")]
    [StringLength(3)]
    public string? CommGroup { get; set; }
    
    [Column("add_user")]
    [StringLength(16)]
    public string? AddUser { get; set; }
    
    [Column("add_date")]
    public DateTime? AddDate { get; set; }
    
    [Column("mod_user")]
    [StringLength(16)]
    public string? ModUser { get; set; }
    
    [Column("mod_date")]
    public DateTime? ModDate { get; set; }
}

[Table("location_types")]
public class LocationType
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = null!;
    
    [Column("transfer")]
    public short? Transfer { get; set; }
    
    [Column("countinv")]
    public short? CountInventory { get; set; }

    public ICollection<Location>? Locations { get; set; }
}

[Table("locations")]
public class Location
{
    [Key]
    [Column("loc_id")]
    public int Id { get; set; }
    
    [Column("name")]
    public string? Name { get; set; }
    
    [Column("loc_type")]
    public string? LocationType { get; set; }
    
    [Column("line_id")]
    public string? LineId { get; set; }
    
    [Column("consumed")]
    public short? Consumed { get; set; }
    
    [Column("berth")]
    public string? Berth { get; set; }
    
    [Column("next_berth")]
    public string? NextBerth { get; set; }
    
    [Column("start_date")]
    public DateTime? StartDate { get; set; }
    
    [Column("foreman")]
    public string? Foreman { get; set; }
    
    [Column("gear")]
    public string? Gear { get; set; }
    
    [Column("cargo")]
    public string? Cargo { get; set; }
    
    [Column("supt")]
    public string? Superintendent { get; set; }
    
    [Column("standby")]
    public string? Standby { get; set; }
    
    [Column("note")]
    public string? Note { get; set; }
    
    [Column("finished")]
    public short? Finished { get; set; }
    
    [Column("voy_no")]
    public string? VoyageNumber { get; set; }
    
    [Column("phone")]
    public string? Phone { get; set; }
    
    [Column("add_user")]
    public string? AddUser { get; set; }
    
    [Column("add_date")]
    public DateTime? AddDate { get; set; }
    
    [Column("mod_user")]
    public string? ModUser { get; set; }
    
    [Column("mod_date")]
    public DateTime? ModDate { get; set; }

    [ForeignKey(nameof(LocationType))]
    public LocationType? Type { get; set; }
    
    [ForeignKey(nameof(Berth))]
    public Berth? BerthNavigation { get; set; }
    
    public ICollection<InventoryLocation>? InventoryLocations { get; set; }
}

[Table("LOCKERINV")]
public class LockerInventory
{
    [Column("ROWNUM")]
    public int? RowNumber { get; set; }
    
    [Column("ID")]
    public string? Id { get; set; }
    
    [Column("QTY")]
    public double? Quantity { get; set; }
    
    [Column("SWL")]
    public string? SWL { get; set; }
}

[Table("testclass")]
public class TestClass
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = null!;
    
    [Column("period")]
    public double? Period { get; set; }
    
    [Column("schedule")]
    public short? Schedule { get; set; }
}

[Table("twoweeks")]
public class TwoWeek
{
    [Key]
    [Column("selectdate")]
    public DateTime SelectDate { get; set; }
}

[Table("recap")]
public class Recap
{
    [Key]
    [Column("invloc_id")]
    public int invlocId { get; set; }
    
    [Column("location")]
    public int? Location { get; set; }
    
    [Column("item")]
    public string? ItemId { get; set; }
    
    [Column("item_name")]
    public string? ItemName { get; set; }

    [Column("onhand")]
    public double? OnHand { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("loc_type")]
    public string? LocationType { get; set; }
    
    [Column("transfer_date")]
    public DateTime? TransferDate { get; set; }

    [Column("quantity")]
    public double? Quantity { get; set; }

    [Column("consumed")]
    public short Consumed { get; set; }

    [Column("inspected_by")]
    public string? InspectedBy { get; set; }
}

[Table("inventory_onsite")]
public class InventoryOnsite
{
    [Key]
    [Column("invloc_id")]
    public int Id { get; set; }
    
    [Column("item")]
    public string? ItemId { get; set; }
    
    [Column("item_name")]
    public string? ItemName { get; set; }
    
    [Column("location")]
    public int? LocationId { get; set; }
    
    [Column("description")]
    public string? Description { get; set; }
    
    [Column("issued")]
    public double? Issued { get; set; }
    
    [Column("onhand")]
    public double? OnHand { get; set; }
    
    [Column("billable")]
    public short? Billable { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Inventory? Inventory { get; set; }

    [ForeignKey(nameof(LocationId))]
    public Location? Location { get; set; }
}

[Table("invtran")]
public class InvTran
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("item")]
    public string? ItemId { get; set; }
    
    [Column("item_name")]
    public string? ItemName { get; set; }
    
    [Column("item_desc")]
    public string? ItemDescription { get; set; }

    [Column("from_location")]
    public int? FromLocation { get; set; }

    [Column("from_name")]
    public string? FromName { get; set; }
    
    [Column("to_location")]
    public int? ToLocation { get; set; }
    
    [Column("to_name")]
    public string? ToName { get; set; }

    [Column("company")]
    public string? Company { get; set; }
    
    [Column("job")]
    public string? Job { get; set; }
    
    [Column("issue_val")]
    public short? IssueValue { get; set; }
    
    [Column("taken_from")]
    public string? TakenFrom { get; set; }
    
    [Column("transfer_date")]
    public DateTime? TransferDate { get; set; }    [Column("quantity")]
    public double? Quantity { get; set; }
    
    [Column("costper")]
    public double? CostPer { get; set; }
    
    [Column("ponum")]
    public string? PONumber { get; set; }
    
    [Column("inspected_by")]
    public string? InspectedBy { get; set; }
    
}

