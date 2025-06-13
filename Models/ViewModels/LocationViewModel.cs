namespace LockerApp.Models.ViewModels;

public class LocationViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string LocationType { get; set; } = "";
    public string Berth { get; set; } = "";
    public string? Gear { get; set; }
    public string? Cargo { get; set; }
    public string? Superintendent { get; set; }
    public string? Foreman { get; set; }
    public string? Standby { get; set; }
    public string? VoyageNumber { get; set; }
    public string? Phone { get; set; }
    public string? Note { get; set; }
    public DateTime? StartDate { get; set; }
} 