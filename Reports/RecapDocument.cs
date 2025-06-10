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
                    .FontSize(10)
                    .FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {            // First header line - Company info and date/page
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("SSA Marine Canada").FontSize(8);
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8));
                    text.Span($"{DateTime.Now:MM/dd/yy h:mm tt}");
                    text.Line("");
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });

            // Second header line
            column.Item().Text("locker:recap").FontSize(8);// Title section with vessel, berth, and starting date
            column.Item().PaddingVertical(5).Row(row =>
            {
                row.RelativeItem(2).Column(c =>
                {
                    c.Item().Text(text =>
                    {
                        text.Span("For the  ").FontSize(10);
                        text.Span(_locationName).Bold().FontSize(10);
                    });
                    c.Item().Text($"               {_voyageNumber}").Bold().FontSize(10);
                });

                row.RelativeItem().AlignLeft().Text(text =>
                {
                    text.Span("Berth  ").FontSize(10);
                    text.Span(_berth).Bold().FontSize(10);
                });
                row.RelativeItem().AlignRight().Text(text =>
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
                _locationName = location.Name ?? "";
                _voyageNumber = location.VoyageNumber ?? "";
                _berth = location.Berth ?? "";
                
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
                InspectedBy = r.InspectedBy ?? ""            })
            .OrderBy(r => r.ItemName)
            .ToListAsync();// If no specific locationId was provided, try to get location info from the first recap item
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
                });                // Table header
                table.Header(header =>
                {
                    header.Cell().Border(0.25f).BorderColor(Colors.Black).AlignCenter().PaddingVertical(2).Text("OUT").Bold();
                    header.Cell().Border(0.25f).BorderColor(Colors.Black).AlignCenter().PaddingVertical(2).Text("").Bold();
                    header.Cell().Border(0.25f).BorderColor(Colors.Black).AlignCenter().PaddingVertical(2).Text("RETN").Bold();
                    header.Cell().Border(0.25f).BorderColor(Colors.Black).AlignCenter().PaddingVertical(2).Text("USED").Bold();
                    header.Cell().Border(0.25f).BorderColor(Colors.Black).AlignCenter().PaddingVertical(2).Text("OTHER").Bold();
                });                // Group items by ItemName to calculate totals
                var groupedItems = _recapItems.GroupBy(r => r.ItemName).ToList();
                
                foreach (var group in groupedItems)
                {
                    var item = group.First(); // Get first item for display info
                    var totalQuantity = group.Sum(g => g.Quantity ?? 0);
                    
                    // Initialize totals
                    double usedTotal = 0;
                    double returnedTotal = 0;
                    double otherTotal = 0;
                    
                    // Apply VBA logic for each item in the group
                    foreach (var recapItem in group)
                    {
                        var quantity = recapItem.Quantity ?? 0;
                        
                        if (recapItem.Consumed == 1)
                        {
                            // If consumed = 1, add to used
                            usedTotal += quantity;
                        }
                        else
                        {
                            // If not consumed, check location type
                            if (recapItem.LocationType == "YARD")
                            {
                                // If location type is YARD, add to returned
                                returnedTotal += quantity;
                            }
                            else
                            {
                                // Otherwise, add to other
                                otherTotal += quantity;
                            }
                        }                    }

                    // Display quantities in appropriate columns
                    var outQty = totalQuantity > 0 ? totalQuantity.ToString("0") : "";
                    var retnQty = returnedTotal > 0 ? returnedTotal.ToString("0") : "";
                    var usedQty = usedTotal > 0 ? usedTotal.ToString("0") : "";
                    var otherQty = otherTotal > 0 ? otherTotal.ToString("0") : "";

                    // Apply VBA logic for item name and description
                    string longName;
                    if (string.IsNullOrWhiteSpace(item.Description))
                    {
                        longName = item.ItemName?.Trim() ?? "";
                    }
                    else
                    {
                        longName = $"{item.ItemName?.Trim()} - {item.Description.Trim()}";
                    }

                    table.Cell().Border(0.25f).BorderColor(Colors.Black).AlignRight().PaddingVertical(1).PaddingRight(5).Text(outQty);
                    table.Cell().Border(0.25f).BorderColor(Colors.Black).PaddingVertical(2).PaddingLeft(5).Text(longName);
                    table.Cell().Border(0.25f).BorderColor(Colors.Black).AlignRight().PaddingVertical(2).PaddingRight(5).Text(retnQty);
                    table.Cell().Border(0.25f).BorderColor(Colors.Black).AlignRight().PaddingVertical(2).PaddingRight(5).Text(usedQty);
                    table.Cell().Border(0.25f).BorderColor(Colors.Black).AlignRight().PaddingVertical(2).PaddingRight(5).Text(otherQty);
                }
            });
        });
    }    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {            column.Item().PaddingTop(20).AlignLeft().Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(10));
                text.Span("Upon return of the above Gear & Equipment, inspection was assigned to the following:");
            });
              column.Item().PaddingTop(15).AlignCenter().Column(col =>
            {
                col.Item().Text("Inspected by:").FontSize(10);
                col.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Black);
                col.Item().PaddingTop(5).Text(_inspectedBy).FontSize(10).AlignCenter();
            });
        });
    }
}