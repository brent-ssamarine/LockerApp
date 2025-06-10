using AccessMigrationApp.Data.LockerDB;
using AccessMigrationApp.Models.LockerDB;
using AccessMigrationApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AccessMigrationApp.Reports;

public class RecapDocument : BaseDocument
{    private readonly DateTime _startDate;
    private readonly string _inspectedBy;
    private readonly int? _locationId;
    private string _locationName = "";
    private string _voyageNumber = "";
    private string _berth = "";
    private DateTime _displayStartDate; // Will be populated from location data or use _startDate as fallback
    private List<RecapViewModel>? _recapItems;

    public RecapDocument(
        IServiceProvider serviceProvider,
        DateTime startDate,
        DateTime endDate,
        string inspectedBy,
        int? locationId = null) : base(serviceProvider)
    {
        _startDate = startDate;
        _inspectedBy = inspectedBy;
        _locationId = locationId;
        _displayStartDate = startDate; // Initialize with the provided start date as fallback
    }

    public async Task PrepareAsync()
    {
        await LoadData();
    }    public override void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x
                    .FontSize(8)
                    .FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            // First header line - Company info and date/page
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("SSA Marine Canada").FontSize(8);
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8));
                    text.Span($"{DateTime.Now:dd-MMM-yy h:mm tt}");
                    text.Span("  Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });

            // Second header line
            column.Item().Text("locker:recap").FontSize(8);            // Title section with vessel, berth, and starting date
            column.Item().PaddingVertical(5).Row(row =>
            {
                row.RelativeItem(2).Column(c =>
                {
                    c.Item().Text(text =>
                    {
                        text.Span("For the  ").FontSize(10);
                        text.Span(_locationName).Bold().FontSize(10);
                    });
                    c.Item().Text(_voyageNumber).FontSize(10);
                });

                row.RelativeItem().AlignCenter().Text(text =>
                {
                    text.Span("Berth  ").FontSize(10);
                    text.Span(_berth).Bold().FontSize(10);
                });                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Starting  ").FontSize(10);
                    text.Span(_displayStartDate.ToString("MM/dd/yyyy")).Bold().FontSize(10);
                });
            });
        });
    }    private async Task LoadData()
    {
        if (_recapItems != null) return; 

        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LockerDbContext>();        // First, load location information if locationId is provided
        if (_locationId.HasValue)
        {
            var location = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == _locationId.Value);

            if (location != null)
            {
                _locationName = location.Name ?? "ARKIS OCEAN";
                _voyageNumber = location.VoyageNumber ?? "99-177";
                _berth = location.Berth ?? "FRAS-4";
                
                // Use the location's StartDate if available, otherwise use the provided _startDate
                if (location.StartDate.HasValue)
                {
                    _displayStartDate = location.StartDate.Value;
                }
            }
        }

        IQueryable<Recap> query = dbContext.Recaps;

        if (_locationId.HasValue)
        {
            query = dbContext.Recaps.Where(r => r.Location == _locationId.Value);
        }

        _recapItems = await query
            .Select(r => new RecapViewModel
            {
                InvLocId = r.invlocId,
                Location = r.Location,
                ItemId = r.ItemId ?? "",
                ItemName = r.ItemName ?? "",
                OnHand = r.OnHand ?? 0,
                Description = r.Description ?? "",
                LocationType = r.LocationType ?? "",
                TransferDate = r.TransferDate,
                Quantity = r.Quantity ?? 0,
                Consumed = r.Consumed,
                InspectedBy = r.InspectedBy ?? ""
            })
            .OrderBy(r => r.TransferDate)
            .ThenBy(r => r.Location)
            .ThenBy(r => r.ItemName)
            .ToListAsync();        // If no specific locationId was provided, try to get location info from the first recap item
        if (!_locationId.HasValue && _recapItems?.Any() == true)
        {
            int? targetLocationId = _recapItems.FirstOrDefault()?.Location;
            
            if (targetLocationId.HasValue)
            {
                var location = await dbContext.Locations
                    .FirstOrDefaultAsync(l => l.Id == targetLocationId.Value);

                if (location != null)
                {
                    _locationName = location.Name ?? "";
                    _voyageNumber = location.VoyageNumber ?? "";
                    _berth = location.Berth ?? "";
                    
                    // Use the location's StartDate if available
                    if (location.StartDate.HasValue)
                    {
                        _displayStartDate = location.StartDate.Value;
                    }
                }
            }
        }
    }
    private void ComposeContent(IContainer container)
    {
        LoadData().Wait();

        if (_recapItems == null || !_recapItems.Any())
        {
            container.Text("No items found.");
            return;
        }

        container.Column(column =>
        {
            // Main table
            column.Item().Table(table =>
            {
                // Define columns similar to the sample image
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);    // OUT
                    columns.RelativeColumn(4);     // Item Description
                    columns.ConstantColumn(40);    // RETN
                    columns.ConstantColumn(40);    // USED
                    columns.ConstantColumn(50);    // OTHER
                });

                // Table header
                table.Header(header =>
                {
                    header.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().Text("OUT").Bold().FontSize(8);
                    header.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().Text("").Bold().FontSize(8);
                    header.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().Text("RETN").Bold().FontSize(8);
                    header.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().Text("USED").Bold().FontSize(8);
                    header.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().Text("OTHER").Bold().FontSize(8);
                });

                // Table rows
                foreach (var item in _recapItems)
                {
                    // Determine which column to show quantity based on Consumed status
                    var outQty = "";
                    var retnQty = "";
                    var usedQty = "";
                    var otherQty = "";

                    if (item.Consumed == 0) // Not consumed - show in RETN
                    {
                        retnQty = item.Quantity?.ToString("0") ?? "";
                    }
                    else if (item.Consumed == 1) // Consumed - show in USED
                    {
                        usedQty = item.Quantity?.ToString("0") ?? "";
                    }
                    else // Other status
                    {
                        otherQty = item.Quantity?.ToString("0") ?? "";
                    }

                    // Show initial quantity in OUT column
                    outQty = item.OnHand?.ToString("0") ?? "";

                    table.Cell().Border(0.5f).BorderColor(Colors.Black).AlignRight().PaddingRight(5).Text(outQty).FontSize(8);
                    table.Cell().Border(0.5f).BorderColor(Colors.Black).PaddingLeft(5).Text($"{item.ItemName} - {item.Description}").FontSize(8);
                    table.Cell().Border(0.5f).BorderColor(Colors.Black).AlignRight().PaddingRight(5).Text(retnQty).FontSize(8);
                    table.Cell().Border(0.5f).BorderColor(Colors.Black).AlignRight().PaddingRight(5).Text(usedQty).FontSize(8);
                    table.Cell().Border(0.5f).BorderColor(Colors.Black).AlignRight().PaddingRight(5).Text(otherQty).FontSize(8);
                }
            });
        });
    }    private void ComposeFooter(IContainer container)
    {
        container.AlignRight().Text(text =>
        {
            text.DefaultTextStyle(x => x.FontSize(8));
            text.Span("Page ");
            text.CurrentPageNumber();
            text.Span(" of ");
            text.TotalPages();
        });
    }
}