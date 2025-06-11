using AccessMigrationApp.Data.LockerDB;
using AccessMigrationApp.Models.LockerDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AccessMigrationApp.Reports;

public class MaterialListDocument : BaseDocument
{
    private readonly int? _locationId;
    private readonly string _locationName;
    private readonly string _berth;
    private readonly DateTime _startDate;
    private List<MaterialList>? _materialItems;

    public MaterialListDocument(
        IServiceProvider serviceProvider,
        int? locationId) : base(serviceProvider)
    {
        _locationId = locationId;
    }

    public async Task PrepareAsync()
    {
        await LoadData();
    }

    public override void Compose(IDocumentContainer container)
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
    }

    private void ComposeHeader(IContainer container)
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
                    text.Span($"{DateTime.Now:MM/dd/yy h:mm tt}");
                    text.Line("");
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });

            // Second header line
            column.Item().Text("locker:materiallist").FontSize(8);

            // Title section with vessel, berth, and starting date
            column.Item().PaddingVertical(5).Row(row =>
            {
                row.RelativeItem().Text(_locationName).Bold().FontSize(12).AlignCenter();
            });

            column.Item().Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Berth  ").FontSize(10);
                    text.Span(_berth).Bold().FontSize(10);
                });
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Started  ").FontSize(10);
                    text.Span(_startDate.ToString("M/d/yyyy")).Bold().FontSize(10);
                });
            });
        });
    }    private async Task LoadData()
    {
        if (_materialItems != null) return;

        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LockerDbContext>();

        // Load material list data based on location
        var query = dbContext.MaterialLists.AsQueryable();

        if (_locationId.HasValue)
        {
            query = query.Where(m => m.LocationId == _locationId.Value);
        }
        else if (!string.IsNullOrEmpty(_locationName))
        {
            query = query.Where(m => m.FromLocationName == _locationName || m.ToLocationName == _locationName);
        }

        _materialItems = await query
            .Where(m => m.InvType == "MATRL") // Only materials
            .OrderBy(m => m.ItemName)
            .ToListAsync();
    }

    private void ComposeContent(IContainer container)
    {
        LoadData().Wait();

        if (_materialItems == null || !_materialItems.Any())
        {
            container.Text("No materials found.");
            return;
        }

        container.Column(column =>
        {
            // Main table
            column.Item().Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);    // Quantity
                    columns.RelativeColumn(4);     // Item Description  
                    columns.ConstantColumn(80);    // MATRL
                });

                // Table header
                table.Header(header =>
                {
                    header.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().PaddingVertical(2).Text("").Bold();
                    header.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().PaddingVertical(2).Text("").Bold();
                    header.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().PaddingVertical(2).Text("MATRL").Bold();
                });

                // Table content - group by item name and sum quantities
                var groupedItems = _materialItems.GroupBy(m => m.ItemName).ToList();
                
                foreach (var group in groupedItems)
                {
                    var item = group.First();
                    var totalQuantity = group.Sum(g => g.Quantity ?? 0);
                    
                    // Apply VBA logic - only show items with quantity > 0
                    if (totalQuantity > 0)
                    {
                        var qtyDisplay = totalQuantity > 0 ? totalQuantity.ToString("0") : "";
                        var itemDisplay = !string.IsNullOrWhiteSpace(item.Description) 
                            ? $"{item.ItemName?.Trim()} - {item.Description.Trim()}"
                            : item.ItemName?.Trim() ?? "";

                        table.Cell().Border(0.5f).BorderColor(Colors.Black).AlignRight().PaddingVertical(2).PaddingRight(5).Text(qtyDisplay);
                        table.Cell().Border(0.5f).BorderColor(Colors.Black).PaddingVertical(2).PaddingLeft(5).Text(itemDisplay);
                        table.Cell().Border(0.5f).BorderColor(Colors.Black).AlignCenter().PaddingVertical(2).Text("MATRL");
                    }
                }
            });

            // Inspection section
            column.Item().PaddingTop(30).Border(0.5f).BorderColor(Colors.Black).Padding(10).Column(innerColumn =>
            {
                innerColumn.Item().Text("Returned Gear/Equipment Inspected By:").Bold().FontSize(10);
                
                innerColumn.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).BorderColor(Colors.Black).Height(25);
                        c.Item().Text("Name").FontSize(8).AlignCenter();
                    });
                    row.ConstantItem(20);
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).BorderColor(Colors.Black).Height(25);
                        c.Item().Text("Name").FontSize(8).AlignCenter();
                    });
                });

                innerColumn.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).BorderColor(Colors.Black).Height(25);
                        c.Item().Text("Name").FontSize(8).AlignCenter();
                    });
                    row.ConstantItem(20);
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(0.5f).BorderColor(Colors.Black).Height(25);
                        c.Item().Text("Name").FontSize(8).AlignCenter();
                    });
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        // Footer is empty in this report format
    }
}
